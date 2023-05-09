using System.Collections.Generic;

namespace J4JSoftware.GeoProcessor;

public class ExportedRoute
{
    public ExportedRoute( 
        List<Coordinate2>? coordinates = null
        )
    {
        Points = coordinates ?? new List<Coordinate2>();
    }

    public string? RouteName { get; set; }
    public string? Description { get; set; }

    public List<Coordinate2> Points { get; }
    public SnapProcessStatus Status { get; set; } = SnapProcessStatus.NoResultsReturned;
}
