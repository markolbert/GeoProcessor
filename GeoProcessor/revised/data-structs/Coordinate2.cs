using System;

namespace J4JSoftware.GeoProcessor;

public class Coordinate2
{
    public Coordinate2(
        double latitude,
        double longitude,
        bool interpolated = false
    )
    {
        Latitude = latitude;
        Longitude = longitude;
        Interpolated = interpolated;
    }

    public double Latitude { get; }
    public double Longitude { get; }
    public bool Interpolated { get; }

    public double? Elevation { get; set; }
    public DateTime? Timestamp { get; set; }
    public string? Description { get; set; }
}