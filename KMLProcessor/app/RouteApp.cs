using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac.Features.Indexed;
using J4JSoftware.Logging;
using Microsoft.Extensions.Hosting;

namespace J4JSoftware.KMLProcessor
{
    public class RouteApp : IHostedService
    {
        private readonly AppConfig _config;
        private readonly IHost _host;
        private readonly IHostApplicationLifetime _lifetime;
        private readonly IIndex<ImportType, IImport> _importers;
        private readonly IExport _exporter;
        private readonly IRouteProcessor _distProc;
        private readonly IRouteProcessor _routeProc;
        private readonly IJ4JLogger _logger;

        public RouteApp(
            IHost host,
            AppConfig config,
            IHostApplicationLifetime lifetime,
            IIndex<ImportType, IImport> importers,
            IIndex<ExportType, IExport> exporters,
            IIndex<ProcessorType, IRouteProcessor> snapProcessors,
            IJ4JLogger logger
        )
        {
            _host = host;
            _config = config;
            _lifetime = lifetime;
            _importers = importers;

            _exporter = exporters[ config.ExportType ];

            _distProc = snapProcessors[ ProcessorType.Distance ];
            _routeProc = snapProcessors[ config.ProcessorType ];

            _logger = logger;
            _logger.SetLoggedType( GetType() );
        }

        public async Task StartAsync( CancellationToken cancellationToken )
        {
            if( !_config.IsValid( _logger ) )
            {
                _lifetime.StopApplication();
                return;
            }

            if( _config.StoreAPIKey )
                return;

            List<KmlDocument>? kDocs = null;

            if( _config.InputFileDetails.Type != ImportType.Unknown )
                kDocs = await LoadFileAsync( _config.InputFileDetails.Type, cancellationToken );

            // if the import based on the extension failed, try all our importers
            if( kDocs == null )
            {
                foreach( var fType in Enum.GetValues<ImportType>()
                    .Where( ft => ft != _config.InputFileDetails.Type && ft != ImportType.Unknown ) )
                {
                    kDocs = await LoadFileAsync( fType, cancellationToken );

                    if( kDocs == null ) 
                        continue;

                    break;
                }
            }

            if( kDocs == null )
            {
                _logger.Error<string>( "Could not load file '{0}'", _config.InputFile! );

                _lifetime.StopApplication();
                return;
            }

            for( var idx = 0; idx < kDocs.Count; idx++ )
            {
                if( await ProcessDocument( kDocs[idx], cancellationToken ) )
                    continue;

                _logger.Error<string, int>("Failed to process KMLDocument '{0}' ({1})", kDocs[idx].RouteName, idx);
                _lifetime.StopApplication();

                return;
            }

            for( var idx = 0; idx < kDocs.Count; idx++ )
            {
                await _exporter.ExportAsync( kDocs[ idx ], idx, cancellationToken );
            }

            _lifetime.StopApplication();
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task StopAsync( CancellationToken cancellationToken )
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            _lifetime.StopApplication();
        }

        private async Task<List<KmlDocument>?> LoadFileAsync( ImportType importType, CancellationToken cancellationToken )
        {
            if( !_importers.TryGetValue( importType, out var importer ) )
                return null;

            return await importer!.ImportAsync( _config.InputFile!, cancellationToken );
        }

        private async Task<bool> ProcessDocument( KmlDocument kDoc, CancellationToken cancellationToken )
        {
            var prevPts = kDoc.Points.Count;

            if (!await RunRouteProcessor(kDoc, _distProc, cancellationToken))
                return false;

            _logger.Information("Reduced points from {0:n0} to {1:n0} by coalescing nearby points",
                prevPts,
                kDoc.Points.Count);

            prevPts = kDoc.Points.Count;

            if (!await RunRouteProcessor(kDoc, _routeProc, cancellationToken))
                return false;

            _logger.Information("Snapping to route changed point count from {0:n0} to {1:n0}",
                prevPts,
                kDoc.Points.Count);

            return true;
        }

        private async Task<bool> RunRouteProcessor( KmlDocument kDoc, IRouteProcessor processor, CancellationToken cancellationToken)
        {
            var routePts = await processor.ProcessAsync(kDoc.Points, cancellationToken);

            if (routePts == null)
            {
                _logger.Error("Route processor '{0}' failed", processor.GetType());
                return false;
            }

            kDoc.Points = routePts;

            return true;
        }

    }
}