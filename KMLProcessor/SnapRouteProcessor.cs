using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BingMapsRESTToolkit;
using J4JSoftware.Logging;

namespace J4JSoftware.KMLProcessor
{
    public abstract class SnapRouteProcessor : ISnapRouteProcessor
    {
        protected SnapRouteProcessor( 
            IAppConfig config,
            IJ4JLogger logger
            )
        {
            Configuration = config;

            Logger = logger;
            Logger.SetLoggedType( this.GetType() );
        }

        protected IJ4JLogger Logger { get; }

        public IAppConfig Configuration { get; }
        public abstract Distance MaxSeparation { get; }
        public abstract int PointsPerRequest { get; }

        public async Task<LinkedList<Coordinate>?> ProcessAsync( 
            LinkedList<Coordinate> nodes,
            CancellationToken cancellationToken )
        {
            var snappedList = new LinkedList<Coordinate>();

            var chunks = ChunkPoints( InterpolatePoints( nodes ) );

            foreach( var chunk in chunks )
            {
                if( !await ProcessChunkAsync( chunk, snappedList, cancellationToken ) )
                    return null;
            }

            return snappedList;
        }

        protected virtual List<Coordinate> InterpolatePoints( LinkedList<Coordinate> nodes )
        {
            var retVal = new List<Coordinate>();

            Coordinate? prevPt = null;

            foreach( var curPt in nodes )
            {
                if( prevPt == null )
                {
                    retVal.Add( curPt );
                    prevPt = curPt;

                    continue;
                }

                var distance = CoordinateExtensions.GetDistance( prevPt, curPt );

                if( distance <= MaxSeparation )
                {
                    retVal.Add( curPt );
                    prevPt = curPt;

                    continue;
                }

                // interpolate
                var numPtsNeeded = Convert.ToInt32(
                    Math.Ceiling( distance.GetValue( MaxSeparation.Unit ) / MaxSeparation.OriginalValue ) );

                var deltaLat = ( curPt.Latitude - prevPt.Latitude ) / numPtsNeeded;
                var deltaLong = ( curPt.Longitude - prevPt.Longitude ) / numPtsNeeded;

                for( var idx = 0; idx < numPtsNeeded; idx++ )
                {
                    retVal.Add( new Coordinate
                    {
                        Latitude = prevPt.Latitude + ( idx + 1 ) * deltaLat,
                        Longitude = prevPt.Longitude + ( idx + 1 ) * deltaLong
                    } );
                }

                prevPt = curPt;
            }

            return retVal;
        }

        protected virtual List<List<Coordinate>> ChunkPoints( List<Coordinate> points )
        {
            var retVal = new List<List<Coordinate>>();

            var ptsChunked = 0;

            while( ptsChunked < points.Count - 1 )
            {
                var chunk = points.Skip( ptsChunked )
                    .Take( PointsPerRequest )
                    .ToList();

                retVal.Add( chunk );

                ptsChunked += chunk.Count;
            }

            return retVal;
        }

        protected virtual async Task<bool> ProcessChunkAsync( 
            List<Coordinate> chunk, 
            LinkedList<Coordinate> outputNodes,
            CancellationToken cancellationToken )
        {
            var snappedPts = await ExecuteRequestAsync( chunk, cancellationToken );

            if( snappedPts == null )
                return false;

            UpdateOutputList( snappedPts!, outputNodes );

            return true;
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        protected virtual async Task<List<Coordinate>?> ExecuteRequestAsync(
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
            List<Coordinate> chunk, 
            CancellationToken cancellationToken )
        {
            return null;
        }

        protected virtual void UpdateOutputList(List<Coordinate> snappedPts, LinkedList<Coordinate> linkedList )
        {
            var prevNode = linkedList.Count == 0 ? null : linkedList.Last;

            foreach( var snappedPt in snappedPts )
            {
                prevNode = prevNode == null
                    ? linkedList.AddFirst(snappedPt)
                    : linkedList.AddAfter(prevNode, snappedPt);
            }
        }
    }
}
