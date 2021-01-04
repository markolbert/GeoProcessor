using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace J4JSoftware.KMLProcessor
{
    public interface IRouteProcessor
    {
        Task<LinkedList<Coordinate>?> ProcessAsync( LinkedList<Coordinate> nodes, CancellationToken cancellationToken );
    }
}