using System.Collections.Generic;

namespace J4JSoftware.GeoProcessor;

public class ImportedRoute
{
    public ImportedRoute()
    {
        Coordinates = new List<Coordinate2>();
    }

    public ImportedRoute(
        string routeName,
        List<Coordinate2> coordinates
    )
    {
        RouteName = routeName;
        Coordinates = coordinates;
    }

    public ImportedRoute Copy() => new( RouteName, new List<Coordinate2>( Coordinates ) );

    public string RouteName { get; set; } = "Unnamed Route";
    public List<Coordinate2> Coordinates { get; }
}