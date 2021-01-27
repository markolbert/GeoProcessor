using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace J4JSoftware.GeoProcessor
{
    public interface IRouteProcessor
    {
        event EventHandler<int>? PointsProcessed; 
        int ReportingInterval { get; set; }
        Task<LinkedList<Coordinate>?> ProcessAsync( LinkedList<Coordinate> nodes, CancellationToken cancellationToken );
    }
}