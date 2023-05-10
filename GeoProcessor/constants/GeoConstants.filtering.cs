namespace J4JSoftware.GeoProcessor;

public partial class GeoConstants
{
    public const double DefaultMaxPointSeparationKm = 2.5;
    public const double DefaultMaxRouteGapMeters = 500;
    public const double DefaultMaxClusterDiameterMeters = 500;
    public const double DefaultMinimumPointGapMeters = 200;
    public const double DefaultMaximumOverallGapMeters = DefaultMinimumPointGapMeters * 5;
    public const double DefaultBearingToleranceDegrees = 15;
    public const double DefaultMaximumBearingDistanceMeters = 2000d;
}
