using System;

namespace J4JSoftware.GeoProcessor;

public class Coordinate2
{
    public Coordinate2(
        double latitude,
        double longitude,
        InterpolationState interpolationState = InterpolationState.NotInterpolated
    )
    {
        Latitude = latitude;
        Longitude = longitude;
        InterpolationState = interpolationState;
    }

    public double Latitude { get; }
    public double Longitude { get; }
    public InterpolationState InterpolationState { get; }

    public double? Elevation { get; set; }
    public DateTime? Timestamp { get; set; }
    public string? Description { get; set; }
}