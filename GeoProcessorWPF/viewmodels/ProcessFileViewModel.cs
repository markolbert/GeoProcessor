using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Autofac.Features.Indexed;
using J4JSoftware.Logging;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;

namespace J4JSoftware.GeoProcessor
{
    public class ProcessFileViewModel : ObservableRecipient, IProcessFileViewModel
    {
        private readonly IAppConfig _appConfig;
        private readonly IIndex<ImportType, IImporter> _importers;
        private readonly IExporter _exporter;
        private readonly IRouteProcessor _distProc;
        private readonly IRouteProcessor _routeProc;
        private readonly IJ4JLogger? _logger;
        private readonly CancellationTokenSource _cancellationSrc = new CancellationTokenSource();

        private int _pointProcessed;
        private string _phase = string.Empty;
        private ProcessState _procState = ProcessState.Ready;
        private string _cmdButtonText = string.Empty;
        private Visibility _cancelVisibility = Visibility.Visible;

        public ProcessFileViewModel(
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
            _appConfig.APIKey = userConfig.GetAPIKey( _appConfig.ProcessorType );
            _appConfig.NetEventChannelConfiguration!.LogEvent += DisplayLogEventAsync;

            _importers = importers;

            _exporter = exporters[ _appConfig.ExportType ];
            _distProc = snapProcessors[ ProcessorType.Distance ];

            _routeProc = snapProcessors[ _appConfig.ProcessorType ];
            _routeProc.PointsProcessed += DisplayPointsProcessedAsync;
            PointsProcessed = 0;

            ProcessCommand = new RelayCommand( ProcessCommandAsync );
            CancelCommand = new RelayCommand( CancelCommandHandler );

            if( string.IsNullOrEmpty( _appConfig.APIKey ) )
            {
                _procState = ProcessState.Aborted;

                _logger?.Error( "{0} API Key is undefined", _appConfig.ProcessorType );

                CommandButtonText = "Close";
                Phase = "Processing not possible";

                return;
            }

            CommandButtonText = "Start";
            Phase = "Ready to begin...";
        }

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

        public string CommandButtonText
        {
            get => _cmdButtonText;
            private set => SetProperty( ref _cmdButtonText, value );
        }

        public Visibility CancelVisibility
        {
            get => _cancelVisibility;
            private set => SetProperty( ref _cancelVisibility, value );
        }

        public ICommand CancelCommand { get; }

        private void CancelCommandHandler()
        {
            Messenger.Send( new ProcessingCompletedMessage( true ), "primary" );
        }

        public ICommand ProcessCommand { get; }

        private async void ProcessCommandAsync()
        {
            switch( _procState )
            {
                case ProcessState.Ready:
                    CancelVisibility = Visibility.Collapsed;
                    _procState = ProcessState.Running;
                    CommandButtonText = "Abort";

                    await Dispatcher.Yield();

                    await ProcessAsync();

                    return;

                case ProcessState.Running:
                    _procState = ProcessState.Aborted;
                    _cancellationSrc.CancelAfter( 2000 );

                    CommandButtonText = "Close";
                    Phase = "Aborted";
                    await Dispatcher.Yield();

                    return;

                case ProcessState.Finished:
                    Messenger.Send( new ProcessingCompletedMessage( true ), "primary" );
                    return;

                case ProcessState.Aborted:
                    Messenger.Send( new ProcessingCompletedMessage( false ), "primary" );
                    return;
            }

            throw new InvalidEnumArgumentException( $"Unsupported {nameof(ProcessState)} '{_procState}'" );
        }

        private async Task ProcessAsync()
        {
            Phase = $"Loading {_appConfig.InputFile.FilePath}";
            await Dispatcher.Yield();

            List<PointSet>? pointSets = null;

            if( _appConfig.InputFile.Type != ImportType.Unknown )
                pointSets = await LoadFileAsync( _appConfig.InputFile.Type, _cancellationSrc.Token );

            if( _cancellationSrc.IsCancellationRequested )
                return;

            // if the import based on the extension failed, try all our importers
            if( pointSets == null )
            {
                foreach( var fType in Enum.GetValues<ImportType>()
                    .Where( ft => ft != _appConfig.InputFile.Type && ft != ImportType.Unknown ) )
                {
                    Phase = $"Loading failed, trying {fType} loader";
                    await Dispatcher.Yield();

                    if( _cancellationSrc.IsCancellationRequested )
                        return;

                    pointSets = await LoadFileAsync( fType, _cancellationSrc.Token );

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
                if( _cancellationSrc.IsCancellationRequested )
                    return;

                if( await ProcessPointSet( pointSets[idx], _cancellationSrc.Token ) )
                    continue;

                _logger?.Error<string, int>("Failed to process KMLDocument '{0}' ({1})", pointSets[idx].RouteName, idx);
                return;
            }

            for( var idx = 0; idx < pointSets.Count; idx++ )
            {
                if( _cancellationSrc.IsCancellationRequested )
                    return;

                Phase = pointSets.Count == 1
                    ? $"Exporting to {_appConfig.OutputFile.FilePath}"
                    : $"Exporting point set #{idx + 1} of {pointSets.Count} to {_appConfig.OutputFile.GetPath( idx )}";

                await _exporter.ExportAsync( pointSets[ idx ], idx, _cancellationSrc.Token );
            }

            _procState = ProcessState.Finished;
            Phase = "Done";
            CommandButtonText = "Close";
            await Dispatcher.Yield();
        }

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

            if (!await RunRouteProcessor(pointSet, _distProc, cancellationToken))
                return false;

            _logger?.Information("Reduced points from {0:n0} to {1:n0} by coalescing nearby points",
                prevPts,
                pointSet.Points.Count);

            if( _cancellationSrc.IsCancellationRequested )
                return false;

            Phase = "Snapping points to route";
            await Dispatcher.Yield();

            prevPts = pointSet.Points.Count;

            if (!await RunRouteProcessor(pointSet, _routeProc, cancellationToken))
                return false;

            _logger?.Information("Snapping to route changed point count from {0:n0} to {1:n0}",
                prevPts,
                pointSet.Points.Count);

            return true;
        }

        private async Task<bool> RunRouteProcessor( PointSet pointSet, IRouteProcessor processor, CancellationToken cancellationToken)
        {
            var routePts = await processor.ProcessAsync(pointSet.Points, cancellationToken);

            if (routePts == null)
            {
                _logger?.Error("Route processor '{0}' failed", processor.GetType());
                return false;
            }

            pointSet.Points = routePts;

            return true;
        }
    }
}
