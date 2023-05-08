using System.Collections.Generic;

namespace J4JSoftware.GeoProcessor;

public interface IImportedRoute : IEnumerable<Coordinate2>
{
    int NumPoints { get; }
    string RouteName { get; }
    string Description { get; }
}
