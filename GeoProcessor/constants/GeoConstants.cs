using System;
using System.Drawing;

namespace J4JSoftware.GeoProcessor;

public partial class GeoConstants
{
    public static TimeSpan DefaultRequestTimeout { get; } = TimeSpan.FromSeconds(20);
    public const int DefaultStatusInterval = 500;
    public static Color DefaultRouteColor { get; }= Color.Blue;
    public static int DefaultRouteWidth = 10;
    public const string DefaultIconSourceHref = "http://maps.google.com/mapfiles/kml/paddle/wht-blank.png";
}
