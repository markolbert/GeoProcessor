#region license

// Copyright 2021 Mark A. Olbert
// 
// This library or program 'GeoProcessorApp' is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public License as
// published by the Free Software Foundation, either version 3 of the License,
// or (at your option) any later version.
// 
// This library or program is distributed in the hope that it will be useful, but
// WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// General Public License for more details.
// 
// You should have received a copy of the GNU General Public License along with
// this library or program.  If not, see <https://www.gnu.org/licenses/>.

#endregion

using System;
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
        private readonly IRouteProcessor _distProc;
        private readonly IExporter _exporter;
        private readonly IIndex<ImportType, IImporter> _importers;
        private readonly IHostApplicationLifetime _lifetime;
        private readonly IJ4JLogger _logger;
        private readonly IRouteProcessor _routeProc;

        private string _processingPhase = string.Empty;
        private int _ptsProcessed;

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
            _distProc.PointsProcessed += PointsProcessedHandler;

            _logger = logger;
            _logger.SetLoggedType( GetType() );

            if( configUpdaters.TryGetValue( AutofacKey, out var updater )
                && updater.Update( _config ) )
            {
                _routeProc = snapProcessors[ config.ProcessorType ];
                _routeProc.PointsProcessed += PointsProcessedHandler;

                return;
            }

            _logger.Fatal( "Incomplete configuration, aborting" );
            _lifetime.StopApplication();
        }

        private void PointsProcessedHandler( object? sender, int pointsProcessed )
        {
            _ptsProcessed += pointsProcessed;
            _logger?.Information( "{0}: {1:n0} points processed", _processingPhase, _ptsProcessed );
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

            List<PointSet>? pointSets = null;

            if( _config.InputFile.Type != ImportType.Unknown )
                pointSets = await LoadFileAsync( _config.InputFile.Type, cancellationToken );

            // if the import based on the extension failed, try all our importers
            if( pointSets == null )
                foreach( var fType in Enum.GetValues<ImportType>()
                    .Where( ft => ft != _config.InputFile.Type && ft != ImportType.Unknown ) )
                {
                    pointSets = await LoadFileAsync( fType, cancellationToken );

                    if( pointSets == null )
                        continue;

                    break;
                }

            if( pointSets == null )
            {
                _logger.Error<string>( "Could not load file '{0}'", _config.InputFile.GetPath() );

                _lifetime.StopApplication();
                return;
            }

            for( var idx = 0; idx < pointSets.Count; idx++ )
            {
                if( await ProcessPointSet( pointSets[ idx ], cancellationToken ) )
                    continue;

                _logger.Error( "Failed to process KMLDocument '{0}' ({1})", pointSets[ idx ].RouteName, idx );
                _lifetime.StopApplication();

                return;
            }

            for( var idx = 0; idx < pointSets.Count; idx++ )
                await _exporter.ExportAsync( pointSets[ idx ], idx, cancellationToken );

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
            _ptsProcessed = 0;
            _processingPhase = "Coalescing";
            var initialPts = pointSet.Points.Count;

            if( !await RunRouteProcessor( pointSet, _distProc, cancellationToken ) )
                return false;

            _logger.Information( "Reduced points from {0:n0} to {1:n0} by coalescing nearby points",
                initialPts,
                pointSet.Points.Count );

            _ptsProcessed = 0;
            _processingPhase = "Snapping to Route";
            initialPts = pointSet.Points.Count;

            if( !await RunRouteProcessor( pointSet, _routeProc, cancellationToken ) )
                return false;

            _logger.Information( "Snapping to route changed point count from {0:n0} to {1:n0}",
                initialPts,
                pointSet.Points.Count );

            return true;
        }

        private async Task<bool> RunRouteProcessor( PointSet pointSet, IRouteProcessor processor,
            CancellationToken cancellationToken )
        {
            var routePts = await processor.ProcessAsync( pointSet.Points, cancellationToken );

            if( routePts == null )
            {
                _logger.Error( "Route processor '{0}' failed", processor.GetType() );
                return false;
            }

            pointSet.Points = routePts;

            return true;
        }
    }
}