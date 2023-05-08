using System;

namespace J4JSoftware.GeoProcessor;

public partial class GeoConstants
{
    public static TimeSpan DefaultRequestTimeout { get; } = TimeSpan.FromSeconds(20);
    public const int DefaultStatusInterval = 500;
}
