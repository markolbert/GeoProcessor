using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using J4JSoftware.Logging;

namespace J4JSoftware.GeoProcessor
{
    public class CloudRouteProcessor : RouteProcessor
    {
        protected CloudRouteProcessor( 
            AppConfig config, 
            IJ4JLogger logger ) 
            : base( config, logger )
        {
        }

        public override async Task<LinkedList<Coordinate>?> ProcessAsync(
            LinkedList<Coordinate> nodes,
            CancellationToken cancellationToken)
        {
            if( string.IsNullOrEmpty( APIKey ) )
            {
                Logger.Error( "{0}: APIKey is undefined", GetType() );
                return null;
            }

            var retVal = new LinkedList<Coordinate>();

            var chunks = ChunkPoints(InterpolatePoints(nodes));

            var pointsProcessed = 0;

            foreach (var coordinates in chunks)
            {
                if (!await ProcessChunkAsync(coordinates, retVal!, cancellationToken))
                    return null;

                pointsProcessed += coordinates.Count;

                if( pointsProcessed % ( 5 * Configuration.MaxPointsPerRequest ) == 0 )
                    Logger.Information( "Processed {0:n0} points via {1}", pointsProcessed, Processor );
            }

            return retVal;
        }

        protected virtual async Task<bool> ProcessChunkAsync(
            List<Coordinate> coordinates,
            LinkedList<Coordinate> outputNodes,
            CancellationToken cancellationToken)
        {
            var snappedPts = await ExecuteRequestAsync(coordinates, cancellationToken);

            if (snappedPts == null)
                return false;

            UpdateOutputList(snappedPts!, outputNodes);

            return true;
        }

        private List<Coordinate> InterpolatePoints(LinkedList<Coordinate> nodes)
        {
            var retVal = new List<Coordinate>();

            Coordinate? prevPt = null;

            foreach (var curPt in nodes)
            {
                if (prevPt == null)
                {
                    retVal.Add(curPt);
                    prevPt = curPt;

                    continue;
                }

                var distance = KMLExtensions.GetDistance(prevPt, curPt);

                if (distance <= Configuration.MaxSeparation)
                {
                    retVal.Add(curPt);
                    prevPt = curPt;

                    continue;
                }

                // interpolate
                var numPtsNeeded = Convert.ToInt32(
                    Math.Ceiling(distance.GetValue(Configuration.MaxSeparation.Unit) / Configuration.MaxSeparation.OriginalValue));

                var deltaLat = (curPt.Latitude - prevPt.Latitude) / numPtsNeeded;
                var deltaLong = (curPt.Longitude - prevPt.Longitude) / numPtsNeeded;

                for (var idx = 0; idx < numPtsNeeded; idx++)
                    retVal.Add(new Coordinate
                    {
                        Latitude = prevPt.Latitude + (idx + 1) * deltaLat,
                        Longitude = prevPt.Longitude + (idx + 1) * deltaLong
                    });

                prevPt = curPt;
            }

            return retVal;
        }

        private List<List<Coordinate>> ChunkPoints(List<Coordinate> points)
        {
            var retVal = new List<List<Coordinate>>();

            var ptsChunked = 0;

            while (ptsChunked < points.Count - 1)
            {
                var coordinates = points.Skip(ptsChunked)
                    .Take(Configuration.MaxPointsPerRequest)
                    .ToList();

                retVal.Add(coordinates);

                ptsChunked += coordinates.Count;
            }

            return retVal;
        }
    }
}