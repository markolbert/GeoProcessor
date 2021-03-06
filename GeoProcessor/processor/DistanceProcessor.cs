﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using J4JSoftware.Logging;

namespace J4JSoftware.GeoProcessor
{
    [RouteProcessor(ProcessorType.Distance)]
    public class DistanceProcessor : RouteProcessor
    {
        public DistanceProcessor(
            IGeoConfig config,
            IJ4JLogger? logger
        )
            : base( config, ProcessorType.Distance, logger )
        {
            Type = GeoExtensions.GetTargetType<RouteProcessorAttribute>(GetType())!.Type;
        }

        public ProcessorType Type { get; }

        public override async Task<LinkedList<Coordinate>?> ProcessAsync(
            LinkedList<Coordinate> nodes,
            CancellationToken cancellationToken)
        {
            var temp = await ExecuteRequestAsync( nodes.ToList(), cancellationToken );

            return temp == null ? null : new LinkedList<Coordinate>( temp );
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        protected override async Task<List<Coordinate>?> ExecuteRequestAsync(
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
            List<Coordinate> coordinates,
            CancellationToken cancellationToken )
        {
            var retVal = new List<Coordinate>();

            switch( coordinates.Count )
            {
                case 0:
                    return retVal;

                case 1:
                    retVal.Add( coordinates[ 0 ] );

                    return retVal;

                default:
                    retVal.Add(coordinates[0]);

                    break;
            }

            var curStartingIdx = 0;
            var ptsSinceLastReport = 0;

            for( var idx = 1; idx < coordinates.Count; idx++ )
            {
                if( cancellationToken.IsCancellationRequested )
                    return null;

                ptsSinceLastReport++;

                if( ptsSinceLastReport >= ReportingInterval )
                {
                    OnReportingInterval( ptsSinceLastReport );
                    ptsSinceLastReport -= ReportingInterval;
                }

                var mostRecentDistance = GeoExtensions
                    .GetDistance(coordinates[idx - 1], coordinates[idx]);

                var distanceFromOrigin = GeoExtensions
                    .GetDistance(coordinates[curStartingIdx], coordinates[idx]);

                if( mostRecentDistance <= Configuration.MaxSeparation
                    && distanceFromOrigin <= Configuration.MaxDistanceMultiplier * Configuration.MaxSeparation )
                    continue;

                retVal.Add( coordinates[ idx ] );
                curStartingIdx = idx;
            }

            return retVal;
        }
    }
}

