using System.Collections.Generic;

namespace J4JSoftware.GeoProcessor;

internal class GoogleResponse
{
    public List<GoogleSnappedPoint> SnappedPoints { get; set; }
    public string WarningMessage { get; set; }
}