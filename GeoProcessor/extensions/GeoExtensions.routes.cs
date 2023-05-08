using System.Linq;

namespace J4JSoftware.GeoProcessor;

public static partial class GeoExtensions
{
    public static Coordinate2 Start( this IImportedRoute route ) => route.First();
    public static Coordinate2 End( this IImportedRoute route ) => route.Last();

    public static Distance2 StartToStart(
        this IImportedRoute route1,
        IImportedRoute route2
    ) =>
        new PointPair( route1.Start(), route2.Start() ).GetDistance();

    public static Distance2 StartToEnd( this IImportedRoute route1, IImportedRoute route2 ) =>
        new PointPair( route1.Start(), route2.End() ).GetDistance();

    public static Distance2 EndToStart( this IImportedRoute route1, IImportedRoute route2) =>
        new PointPair( route1.End(), route2.Start() ).GetDistance();

    public static Distance2 EndToEnd( this IImportedRoute route1, IImportedRoute route2 ) =>
        new PointPair( route1.End(), route2.End() ).GetDistance();
}
