using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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

        private ProcessorState _procState;
        private int _pointProcessed;
        private string _phase = string.Empty;

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
            _appConfig.NetEventChannelConfiguration!.LogEvent += DisplayLogEventAsync;

            _importers = importers;

            _exporter = exporters[ _appConfig.ExportType ];
            _distProc = snapProcessors[ ProcessorType.Distance ];
            _routeProc = snapProcessors[ _appConfig.ProcessorType ];

            // processing status
            _routeProc.PointsProcessed += DisplayPointsProcessedAsync;
            PointsProcessed = 0;

            AbortCommand = new RelayCommand( AbortCommandHandler );

            _appConfig.APIKey = userConfig.GetAPIKey( _appConfig.ProcessorType );

            if( string.IsNullOrEmpty( _appConfig.APIKey ) )
            {
                ProcessorState = GeoProcessor.ProcessorState.Aborted;

                _logger?.Error( "{0} API Key is undefined", _appConfig.ProcessorType );

                Phase = "Processing not possible";
                ProcessorState = GeoProcessor.ProcessorState.Aborted;
            }
            else ProcessorState = GeoProcessor.ProcessorState.Ready;
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

        public ProcessorState ProcessorState
        {
            get => _procState;

            private set
            {
                _procState = value;

                Messenger.Send( new ProcessorStateMessage( _procState ), "primary" );
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

        public ICommand AbortCommand { get; }

        private void AbortCommandHandler()
        {
            ProcessorState = ProcessorState.Aborted;

            _cancellationSrc.Cancel();
        }

        public async Task ProcessAsync()
        {
            if( ProcessorState == ProcessorState.Aborted )
                return;

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

            ProcessorState = ProcessorState.Finished;
            Phase = "Done";
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
