#region license

// Copyright 2021 Mark A. Olbert
// 
// This library or program 'GeoProcessor' is free software: you can redistribute it
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
using System.Threading;
using System.Threading.Tasks;
using J4JSoftware.Logging;

namespace J4JSoftware.GeoProcessor
{
    public class RouteProcessor : IRouteProcessor
    {
        private int _reportingInterval;

        protected RouteProcessor(
            IGeoConfig config,
            ProcessorType processorType,
            J4JLogger? logger )
        {
            Configuration = config.ProcessorInfo!;
            Processor = config.ProcessorType;
            ProcessorType = processorType;

            ReportingInterval = ProcessorType.MaxPointsPerRequest() == int.MaxValue
                ? 500
                : ProcessorType.MaxPointsPerRequest() * 5;

            Logger = logger;
            Logger?.SetLoggedType( GetType() );
        }

        protected J4JLogger? Logger { get; }
        protected ProcessorType ProcessorType { get; }

        protected ProcessorInfo Configuration { get; }
        protected ProcessorType Processor { get; }

        public event EventHandler<int>? PointsProcessed;

        public int ReportingInterval
        {
            get => _reportingInterval;

            set
            {
                if( value <= 0 )
                {
                    var temp = ProcessorType.MaxPointsPerRequest() == int.MaxValue
                        ? 500
                        : ProcessorType.MaxPointsPerRequest() * 5;

                    _reportingInterval = temp;

                    Logger?.Error( "ReportingInterval must be >= 0, defaulting to {0}", temp );
                }
                else
                {
                    _reportingInterval = value;
                }
            }
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public virtual async Task<LinkedList<Coordinate>?> ProcessAsync( LinkedList<Coordinate> nodes,
            CancellationToken cancellationToken )
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            return null;
        }

        protected void OnReportingInterval( int points )
        {
            PointsProcessed?.Invoke( this, points );
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        protected virtual async Task<List<Coordinate>?> ExecuteRequestAsync(
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
            List<Coordinate> coordinates,
            CancellationToken cancellationToken )
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
    }
}