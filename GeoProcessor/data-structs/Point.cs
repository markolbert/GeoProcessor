using System;

namespace J4JSoftware.GeoProcessor;

public class Point
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double? Elevation { get; set; }
    public DateTime? Timestamp { get; set; }
    public string? Description { get; set; }
}
