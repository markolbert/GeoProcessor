#pragma warning disable CS8618
namespace J4JSoftware.GeoProcessor;

internal class GoogleSnappedPoint
{
    public GoogleLatLong Location { get; set; }
    public string PlaceId { get; set; }
    public int OriginalIndex { get; set; }
}
