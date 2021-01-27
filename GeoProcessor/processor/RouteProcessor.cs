using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using J4JSoftware.Logging;

namespace J4JSoftware.GeoProcessor
{
    public class RouteProcessor : IRouteProcessor
    {
        public event EventHandler<int>? PointsProcessed;

        private int _reportingInterval;

        protected RouteProcessor(
            IGeoConfig config,
            IJ4JLogger? logger )
        {
            Configuration = config.ProcessorInfo;
            Processor = config.ProcessorType;
            ReportingInterval = config.ProcessorInfo.MaxPointsPerRequest * 5;

            Logger = logger;
            Logger?.SetLoggedType( GetType() );
        }

        protected IJ4JLogger? Logger { get; }

        public int ReportingInterval
        {
            get => _reportingInterval;

            set
            {
                if( value <= 0 )
                {
                    var temp = Configuration.MaxPointsPerRequest * 5;
                    _reportingInterval = temp;

                    Logger?.Error( "ReportingInterval must be >= 0, defaulting to {0}", temp );
                }
                else _reportingInterval = value;
            }
        }

        protected void OnReportingInterval( int points ) => PointsProcessed?.Invoke( this, points );

        protected ProcessorInfo Configuration { get; }
        protected ProcessorType Processor { get; }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        protected virtual async Task<List<Coordinate>?> ExecuteRequestAsync(
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
            List<Coordinate> coordinates,
            CancellationToken cancellationToken)
        {
            return null;
        }

        protected virtual void UpdateOutputList( List<Coordinate> snappedPts, LinkedList<Coordinate> linkedList )
        {
            var prevNode = linkedList.Count == 0 ? null : linkedList.Last;

            foreach( var snappedPt in snappedPts )
                prevNode = prevNode == null
                    ? linkedList.AddFirst( snappedPt )
                    : linkedList.AddAfter( prevNode, snappedPt );
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public virtual async Task<LinkedList<Coordinate>?> ProcessAsync( LinkedList<Coordinate> nodes, CancellationToken cancellationToken )
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            return null;
        }
    }
}