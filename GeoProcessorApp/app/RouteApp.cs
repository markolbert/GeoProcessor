﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac.Features.Indexed;
using J4JSoftware.ConsoleUtilities;
using J4JSoftware.Logging;
using Microsoft.Extensions.Hosting;
#pragma warning disable 8618

namespace J4JSoftware.GeoProcessor
{
    public class RouteApp : IHostedService
    {
        internal const string AutofacKey = "RouteApp";

        private readonly AppConfig _config;
        private readonly IHostApplicationLifetime _lifetime;
        private readonly IIndex<ImportType, IImporter> _importers;
        private readonly IExporter _exporter;
        private readonly IRouteProcessor _distProc;
        private readonly IRouteProcessor _routeProc;
        private readonly IJ4JLogger _logger;

        public RouteApp(
            AppConfig config,
            IHostApplicationLifetime lifetime,
            IIndex<string, IConfigurationUpdater> configUpdaters,
            IIndex<ImportType, IImporter> importers,
            IIndex<ExportType, IExporter> exporters,
            IIndex<ProcessorType, IRouteProcessor> snapProcessors,
            IJ4JLogger logger
        )
        {
            _config = config;
            _lifetime = lifetime;
            _importers = importers;

            _exporter = exporters[ config.ExportType ];
            _distProc = snapProcessors[ ProcessorType.Distance ];

            _logger = logger;
            _logger.SetLoggedType( GetType() );

            if( configUpdaters.TryGetValue( AutofacKey, out var updater )
                && updater.Update( _config ) )
            {
                _routeProc = snapProcessors[ config.ProcessorType ];
                return;
            }

            _logger.Fatal( "Incomplete configuration, aborting" );
            _lifetime.StopApplication();
        }

        public async Task StartAsync( CancellationToken cancellationToken )
        {
            if( !_config.IsValid(_logger) )
            {
                _lifetime.StopApplication();
                return;
            }

            if( _config.StoreAPIKey )
                return;

            List<PointSet>? pointSets = null;

            if( _config.InputFile.Type != ImportType.Unknown )
                pointSets = await LoadFileAsync( _config.InputFile.Type, cancellationToken );

            // if the import based on the extension failed, try all our importers
            if( pointSets == null )
            {
                foreach( var fType in Enum.GetValues<ImportType>()
                    .Where( ft => ft != _config.InputFile.Type && ft != ImportType.Unknown ) )
                {
                    pointSets = await LoadFileAsync( fType, cancellationToken );

                    if( pointSets == null ) 
                        continue;

                    break;
                }
            }

            if( pointSets == null )
            {
                _logger.Error<string>( "Could not load file '{0}'", _config.InputFile.GetPath() );

                _lifetime.StopApplication();
                return;
            }

            for( var idx = 0; idx < pointSets.Count; idx++ )
            {
                if( await ProcessPointSet( pointSets[idx], cancellationToken ) )
                    continue;

                _logger.Error<string, int>("Failed to process KMLDocument '{0}' ({1})", pointSets[idx].RouteName, idx);
                _lifetime.StopApplication();

                return;
            }

            for( var idx = 0; idx < pointSets.Count; idx++ )
            {
                await _exporter.ExportAsync( pointSets[ idx ], idx, cancellationToken );
            }

            _lifetime.StopApplication();
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task StopAsync( CancellationToken cancellationToken )
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            _lifetime.StopApplication();
        }

        private async Task<List<PointSet>?> LoadFileAsync( ImportType importType, CancellationToken cancellationToken )
        {
            if( !_importers.TryGetValue( importType, out var importer ) )
                return null;

            return await importer!.ImportAsync( _config.InputFile.GetPath(), cancellationToken );
        }

        private async Task<bool> ProcessPointSet( PointSet pointSet, CancellationToken cancellationToken )
        {
            var prevPts = pointSet.Points.Count;

            if (!await RunRouteProcessor(pointSet, _distProc, cancellationToken))
                return false;

            _logger.Information("Reduced points from {0:n0} to {1:n0} by coalescing nearby points",
                prevPts,
                pointSet.Points.Count);

            prevPts = pointSet.Points.Count;

            if (!await RunRouteProcessor(pointSet, _routeProc, cancellationToken))
                return false;

            _logger.Information("Snapping to route changed point count from {0:n0} to {1:n0}",
                prevPts,
                pointSet.Points.Count);

            return true;
        }

        private async Task<bool> RunRouteProcessor( PointSet pointSet, IRouteProcessor processor, CancellationToken cancellationToken)
        {
            var routePts = await processor.ProcessAsync(pointSet.Points, cancellationToken);

            if (routePts == null)
            {
                _logger.Error("Route processor '{0}' failed", processor.GetType());
                return false;
            }

            pointSet.Points = routePts;

            return true;
        }

    }
}