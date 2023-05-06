using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.GeoProcessor;

[ImportFilter("Assemble Route")]
public class AssembleRoute : ImportFilter
{
    private record RouteEndPoints( ImportedRoute Route )
    {
        public Coordinate2 Start => Route.Coordinates.First();
        public Coordinate2 End => Route.Coordinates.Last();
    }

    private enum SegmentConnection
    {
        None,
        Multiple,
        StartToStart,
        StartToEnd,
        EndToStart,
        EndToEnd
   }

    private double _maxRouteGap = GeoConstants.DefaultMaxRouteGapMeters;
    private RouteEndPoints? _mergedRoute;

    public AssembleRoute(
        ILoggerFactory? loggerFactory
    )
    :base( loggerFactory)
    {
    }

    public double MaximumRouteGap
    {
        get => _maxRouteGap;
        set => _maxRouteGap = value <= 0 ? GeoConstants.DefaultMaxRouteGapMeters : value;
    }

    public bool RemoveUnconnectedRoutes { get; set; }

    public override List<ImportedRoute> Filter( List<ImportedRoute> input )
    {
         var filteredInput = input.Where( x => x.Coordinates.Count > 1 )
                                 .Select( x => new RouteEndPoints( x ) )
                                 .ToList();

        if( filteredInput.Count != input.Count )
            Logger?.LogTrace("Ignoring routes with less than 2 points");

        var unmerged = MergeRoutes( filteredInput );

        var retVal = new List<ImportedRoute>();

        if( _mergedRoute != null )
            retVal.Add( _mergedRoute.Route );

        if( !RemoveUnconnectedRoutes )
            retVal.AddRange( unmerged.Select( x => x.Route ) );

        return retVal;
    }

    private List<RouteEndPoints> MergeRoutes( List<RouteEndPoints> routes )
    {
        if( !routes.Any() )
            return routes;

        if( _mergedRoute != null && MergeRoute( routes, _mergedRoute, -1 ) )
            return MergeRoutes( routes.ToList() );

        for( var idx = 0; idx < routes.Count; ++idx )
        {
            if( !MergeRoute( routes, routes[ idx ], idx ) )
                continue;

            return MergeRoutes(routes.ToList());
        }

        return routes;
    }

    private bool MergeRoute( List<RouteEndPoints> routes, RouteEndPoints baseRoute, int routeIndex )
    {
        RouteEndPoints? adjacentRoute = null;

        var connection = SegmentConnection.None;

        for( var idx = 0; idx < routes.Count; idx++ )
        {
            if( idx == routeIndex )
                continue;

            var curConnection = RouteConnection( baseRoute, routes[ idx ] );

            switch( curConnection )
            {
                case SegmentConnection.None:
                    // no op
                    break;

                case SegmentConnection.Multiple:
                    // unexpected, not allowed
                    Logger?.LogError( "Unexpected segment connection value '{connection}'", curConnection );
                    break;

                default:
                    if( connection == SegmentConnection.None )
                    {
                        connection = curConnection;
                        adjacentRoute = routes[ idx ];
                    }
                    else connection = SegmentConnection.Multiple;

                    break;
            }
        }

        if( adjacentRoute == null )
            return false;

        if( _mergedRoute == null )
            AddPointsToMergedRoute( baseRoute );

        AddPointsToMergedRoute( adjacentRoute );

        return true;
    }

    private void AddPointsToMergedRoute( RouteEndPoints route  )
    {
        _mergedRoute ??= new RouteEndPoints(new ImportedRoute("Merged Route", new List<Coordinate2>()));
        _mergedRoute.Route.Coordinates.AddRange( route.Route.Coordinates );
    }

    private SegmentConnection RouteConnection( RouteEndPoints current, RouteEndPoints other )
    {
        var gaps = new Dictionary<SegmentConnection, double>
        {
            { SegmentConnection.StartToStart, current.Start.GetDistance( other.Start ) },
            { SegmentConnection.StartToEnd, current.Start.GetDistance( other.End ) },
            { SegmentConnection.EndToStart, current.End.GetDistance( other.Start ) },
            { SegmentConnection.EndToEnd, current.End.GetDistance( other.End ) },
        };

        var minGap = double.MaxValue;
        var minConnection = SegmentConnection.None;

        foreach( var kvp in gaps )
        {
            if( !( kvp.Value < minGap ) )
                continue;

            minGap = kvp.Value;
            minConnection = kvp.Key;
        }

        return minGap <= MaximumRouteGap ? minConnection : SegmentConnection.None;
    }
}
