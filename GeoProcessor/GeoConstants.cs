using System;

namespace J4JSoftware.GeoProcessor;

public class GeoConstants
{
    public const double RadiansPerDegree = Math.PI / 180;
    public const double DegreesPerRadian = 180 / Math.PI;
    public const double EarthRadiusInMiles = 3958.8;
    public const double EarthRadiusInKilometers = 6371;

    public const double RouteGapEqualityTolerance = 0.01;

    public const double DefaultMaxPointSeparationKm = 2.5;
    public const double DefaultMaxRouteGapMeters = 500;
    public const double DefaultMaxClusterDiameterMeters = 500;
    public const double DefaultMinimumPointGapMeters = 200;
    public const double DefaultMaximumOverallGapMeters = DefaultMinimumPointGapMeters * 5;

    public static TimeSpan DefaultRequestTimeout { get; } = TimeSpan.FromSeconds(20);

    public const int MinimumRouteWidth = 1;
    public const int DefaultStatusInterval = 500;

    internal const string TrackName = "trk";
    internal const string RouteName = "name";
    internal const string TrackSegmentName = "trkSeg";
    internal const string MessageName = "desc";
    internal const string TimeName = "time";
    internal const string ElevationName = "ele";
    internal const string TrackPointName = "trkpt";
    internal const string LongitudeName = "lon";
    internal const string LatitudeName = "lat";
}
