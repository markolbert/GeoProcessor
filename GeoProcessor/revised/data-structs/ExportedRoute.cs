using System.Collections.Generic;

namespace J4JSoftware.GeoProcessor;

public record ExportedRoute( string FolderName, List<Coordinate2>? Coordinates, SnapProcessStatus Status );
