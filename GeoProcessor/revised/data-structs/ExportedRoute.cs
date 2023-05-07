using System.Collections.Generic;

namespace J4JSoftware.GeoProcessor;

public class ExportedRoute
{
    public ExportedRoute()
    {
        Coordinates = new List<Coordinate2>();
    }

    public ExportedRoute( 
        string folderName, 
        List<Coordinate2>? coordinates, 
        SnapProcessStatus status 
        )
    {
        FolderName = folderName;
        Coordinates = coordinates;
        Status = status;
    }

    public string FolderName { get; set; } = "Unnamed Route";
    public List<Coordinate2>? Coordinates { get; }
    public SnapProcessStatus Status { get; set; } = SnapProcessStatus.NoResultsReturned;
}
