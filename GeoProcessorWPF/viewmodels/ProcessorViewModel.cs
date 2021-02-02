using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using Autofac.Features.Indexed;
using J4JSoftware.Logging;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;

namespace J4JSoftware.GeoProcessor
{
    public class ProcessorViewModel : ObservableRecipient, IProcessorViewModel
    {
        private readonly IAppConfig _appConfig;
        private readonly IUserConfig _userConfig;
        private readonly IIndex<ImportType, IImporter> _importers;
        private readonly IIndex<ExportType, IExporter> _exporters;
        private readonly IIndex<ProcessorType, IRouteProcessor> _snapProcessors;

        private readonly IJ4JLogger? _logger;
        
        private ProcessorState _procState;
        private int _pointProcessed;
        private string _phase = string.Empty;
        private CancellationTokenSource? _cancellationSrc;

        public ProcessorViewModel(
            IAppConfig appConfig,
            IUserConfig userConfig,
            IIndex<ImportType, IImporter> importers,
            IIndex<ExportType, IExporter> exporters,
            IIndex<ProcessorType, IRouteProcessor> snapProcessors,
            IJ4JLogger? logger )
        {
            _logger = logger;
            _logger?.SetLoggedType( GetType() );

            _appConfig = appConfig;
            _appConfig.NetEventChannelConfiguration!.LogEvent += DisplayLogEventAsync;

            _userConfig = userConfig;

            _importers = importers;
            _exporters = exporters;
            _snapProcessors = snapProcessors;

            AbortCommand = new RelayCommand<ProcessWindow>( AbortCommandHandler );
            WindowLoadedCommand = new AsyncRelayCommand( WindowLoadedCommandAsync );
        }

        #region event handlers
        
        private async void DisplayPointsProcessedAsync( object? sender, int points )
        {
            PointsProcessed += points;
            await Dispatcher.Yield();
        }

        private async void DisplayLogEventAsync( object? sender, NetEventArgs e )
        {
            Messages.Add( e.LogMessage );
            await Dispatcher.Yield();
        }

        //// event fires when window is loaded and ready to start processing
        //public async Task OnWindowLoadedAsync()
        //{
        //    _appConfig.APIKey = _userConfig.GetAPIKey( _appConfig.ProcessorType );

        //    if( string.IsNullOrEmpty( _appConfig.APIKey ) )
        //    {
        //        ProcessorState = ProcessorState.Aborted;

        //        _logger?.Error( "{0} API Key is undefined", _appConfig.ProcessorType );

        //        Phase = "Processing not possible";
        //        ProcessorState = ProcessorState.Aborted;

        //        return;
        //    }
            
        //    ProcessorState = ProcessorState.Ready;
        //    PointsProcessed = 0;
        //    Messages.Clear();
        //    OnPropertyChanged( nameof(Messages) );
            
        //    await Dispatcher.Yield();
        //    _cancellationSrc = new CancellationTokenSource();

        //    await ProcessAsync( _cancellationSrc.Token );
        //}

        #endregion

        public ProcessorState ProcessorState
        {
            get => _procState;

            private set
            {
                _procState = value;
                Messenger.Send( new ProcessorStateMessage( _procState ), "primary" );

                Dispatcher.Yield();
            }
        }

        public string Phase
        {
            get => _phase;
            set => SetProperty( ref _phase, value );
        }

        public int PointsProcessed
        {
            get => _pointProcessed;
            private set => SetProperty( ref _pointProcessed, value );
        }

        public ObservableCollection<string> Messages { get; } = new();

        #region Commands

        public ICommand AbortCommand { get; }

        private void AbortCommandHandler( ProcessWindow procWin )
        {
            _cancellationSrc?.Cancel();

            ProcessorState = ProcessorState.Aborted;

            Messenger.Send( new CloseModalWindowMessage( DialogWindow.Processor ), "primary" );
        }

        public ICommand WindowLoadedCommand { get; }

        private async Task WindowLoadedCommandAsync()
        {
            _appConfig.APIKey = _userConfig.GetAPIKey( _appConfig.ProcessorType );

            if( string.IsNullOrEmpty( _appConfig.APIKey ) )
            {
                ProcessorState = ProcessorState.Aborted;

                _logger?.Error( "{0} API Key is undefined", _appConfig.ProcessorType );

                Phase = "Processing not possible";
                ProcessorState = ProcessorState.Aborted;

                return;
            }
            
            ProcessorState = ProcessorState.Ready;
            PointsProcessed = 0;
            Messages.Clear();
            OnPropertyChanged( nameof(Messages) );
            
            await Dispatcher.Yield();
            _cancellationSrc = new CancellationTokenSource();

            await ProcessAsync( _cancellationSrc.Token );
        }

        public async Task ProcessAsync( CancellationToken cancellationToken )
        {
            if( ProcessorState == ProcessorState.Aborted )
                return;

            Phase = $"Loading {_appConfig.InputFile.FilePath}";
            await Dispatcher.Yield();

            List<PointSet>? pointSets = null;

            if( _appConfig.InputFile.Type != ImportType.Unknown )
                pointSets = await LoadFileAsync( _appConfig.InputFile.Type, cancellationToken );

            if( cancellationToken.IsCancellationRequested )
                return;

            // if the import based on the extension failed, try all our importers
            if( pointSets == null )
            {
                foreach( var fType in Enum.GetValues<ImportType>()
                    .Where( ft => ft != _appConfig.InputFile.Type && ft != ImportType.Unknown ) )
                {
                    Phase = $"Loading failed, trying {fType} loader";
                    await Dispatcher.Yield();

                    if( cancellationToken.IsCancellationRequested )
                        return;

                    pointSets = await LoadFileAsync( fType, cancellationToken );

                    if( pointSets == null ) 
                        continue;

                    break;
                }
            }

            if( pointSets == null )
            {
                _logger?.Error<string>( "Could not load file '{0}'", _appConfig.InputFile.GetPath() );
                return;
            }

            for( var idx = 0; idx < pointSets.Count; idx++ )
            {
                if( cancellationToken.IsCancellationRequested )
                    return;

                if( await ProcessPointSet( pointSets[idx], cancellationToken ) )
                    continue;

                _logger?.Error<string, int>("Failed to process KMLDocument '{0}' ({1})", pointSets[idx].RouteName, idx);
                return;
            }

            var exporter = _exporters[ _appConfig.ExportType ];

            for( var idx = 0; idx < pointSets.Count; idx++ )
            {
                if( cancellationToken.IsCancellationRequested )
                    return;

                Phase = pointSets.Count == 1
                    ? $"Exporting to {_appConfig.OutputFile.FilePath}"
                    : $"Exporting point set #{idx + 1} of {pointSets.Count} to {_appConfig.OutputFile.GetPath( idx )}";

                await exporter.ExportAsync( pointSets[ idx ], idx, cancellationToken );
            }

            ProcessorState = ProcessorState.Finished;
            Phase = "Done";
            await Dispatcher.Yield();

            Messenger.Send( new CloseModalWindowMessage( DialogWindow.Processor ), "primary" );
        }

        #endregion

        #region Route processing methods

        private async Task<List<PointSet>?> LoadFileAsync( ImportType importType, CancellationToken cancellationToken )
        {
            if( !_importers.TryGetValue( importType, out var importer ) )
                return null;

            return await importer!.ImportAsync( _appConfig.InputFile.GetPath(), cancellationToken );
        }

        private async Task<bool> ProcessPointSet( PointSet pointSet, CancellationToken cancellationToken )
        {
            Phase = "Coalescing points by distance";
            await Dispatcher.Yield();

            var prevPts = pointSet.Points.Count;

            if (!await RunRouteProcessor(pointSet, _snapProcessors[ ProcessorType.Distance ], cancellationToken))
                return false;

            _logger?.Information("Reduced points from {0:n0} to {1:n0} by coalescing nearby points",
                prevPts,
                pointSet.Points.Count);

            if( cancellationToken.IsCancellationRequested )
                return false;

            Phase = "Snapping points to route";
            await Dispatcher.Yield();

            prevPts = pointSet.Points.Count;

            if (!await RunRouteProcessor(pointSet, _snapProcessors[ _appConfig.ProcessorType ], cancellationToken))
                return false;

            _logger?.Information("Snapping to route changed point count from {0:n0} to {1:n0}",
                prevPts,
                pointSet.Points.Count);

            return true;
        }

        private async Task<bool> RunRouteProcessor( PointSet pointSet, IRouteProcessor processor, CancellationToken cancellationToken)
        {
            PointsProcessed = 0;
            processor.PointsProcessed += DisplayPointsProcessedAsync;

            var routePts = await processor.ProcessAsync(pointSet.Points, cancellationToken);

            processor.PointsProcessed -= DisplayPointsProcessedAsync;

            if (routePts == null)
            {
                _logger?.Error("Route processor '{0}' failed", processor.GetType());
                return false;
            }

            pointSet.Points = routePts;

            return true;
        }

        #endregion
    }
}
