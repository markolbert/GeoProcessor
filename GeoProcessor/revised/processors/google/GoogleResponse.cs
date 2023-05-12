using System.Collections.Generic;
#pragma warning disable CS8618

namespace J4JSoftware.GeoProcessor;

internal class GoogleResponse
{
    public List<GoogleSnappedPoint> SnappedPoints { get; set; }
    public string WarningMessage { get; set; }
}