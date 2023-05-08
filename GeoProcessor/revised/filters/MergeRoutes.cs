using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.GeoProcessor;

[BeforeAllImportFilter("Merge Routes", 1)]
public class MergeRoutes : ImportFilter
{
    private double _maxRouteGap = GeoConstants.DefaultMaxRouteGapMeters;

    public MergeRoutes(
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

    public override List<ImportedRoute> Filter( List<ImportedRoute> input )
    {
        if( input.Count <= 1 )
            return input;

        var filteredInput = input.Where( x => x.Points.Count > 1 )
                                 .ToList();

        if( filteredInput.Count != input.Count )
            Logger?.LogTrace( "Ignoring routes with less than 2 points" );

        var retVal = new List<ImportedRoute>();

        var connections = GetConnections(filteredInput);
        var prevConnections = 0;
        var curConnections = connections.Count;

        while ( filteredInput.Any() && prevConnections != curConnections )
        {
            var curSet = connections.First();

            var adjacentRoutes = curSet.GetClosest( MaximumRouteGap, GeoConstants.RouteGapEqualityTolerance );

            if( adjacentRoutes.Count == 0 )
            {
                // routes without any adjacent routes shouldn't be merged, so add them
                // to the return collection
                retVal.Add( filteredInput[ curSet.RouteIndex ] );

                // remove this route from the input set
                filteredInput.RemoveAt( curSet.RouteIndex );
            }
            else
            {
                var adjacent = adjacentRoutes[ 0 ];

                // create a merged route using the two routes, honoring the connection
                var mergedRoute = CreateMergedRoute( filteredInput[ curSet.RouteIndex ],
                                                     filteredInput[ adjacent.ConnectedRouteIndex ],
                                                     adjacent.Type );

                // remove the two routes from the input set
                foreach( var toRemove in new[] { curSet.RouteIndex, adjacent.ConnectedRouteIndex }
                           .OrderByDescending( x => x ) )
                {
                    filteredInput.RemoveAt( toRemove );
                }

                // add the newly-created route
                filteredInput.Add( mergedRoute );
            }

            prevConnections = curConnections;
            connections = GetConnections( filteredInput );
            curConnections = connections.Count;
        }

        // add any remaining filterInput entries to the return collection
        retVal.AddRange( filteredInput );

        return retVal;
    }

    private static List<RouteConnections> GetConnections( List<ImportedRoute> routes )
    {
        var retVal = new List<RouteConnections>();

        for( var outerIdx = 0; outerIdx < routes.Count; outerIdx++ )
        {
            var curSet = new RouteConnections( outerIdx, new List<RouteConnection>() );

            for( var innerIdx = 0; innerIdx < routes.Count; innerIdx++ )
            {
                if( innerIdx == outerIdx )
                    continue;

                curSet.Connections.Add( new RouteConnection( innerIdx,
                                                             RouteConnectionType.StartToStart,
                                                             routes[ outerIdx ]
                                                                .StartToStart( routes[ innerIdx ] )
                                                           * 1000 ) );

                curSet.Connections.Add( new RouteConnection( innerIdx,
                                                             RouteConnectionType.StartToEnd,
                                                             routes[ outerIdx ]
                                                                .StartToEnd( routes[ innerIdx ] )
                                                           * 1000 ) );

                curSet.Connections.Add( new RouteConnection( innerIdx,
                                                             RouteConnectionType.EndToStart,
                                                             routes[ outerIdx ].EndToStart( routes[ innerIdx ] )
                                                           * 1000 ) );

                curSet.Connections.Add( new RouteConnection( innerIdx,
                                                             RouteConnectionType.EndToEnd,
                                                             routes[ outerIdx ].EndToEnd( routes[ innerIdx ] )
                                                           * 1000 ) );
            }

            retVal.Add( curSet );
        }

        return retVal;
    }

    private static ImportedRoute CreateMergedRoute(
        ImportedRoute route1,
        ImportedRoute route2,
        RouteConnectionType connectionType
    )
    {
        var retVal = new ImportedRoute( route1.Points.ToList() ) { RouteName = "Merged Route" };

        // how we add the new points depends on which end of the merged route they're connected to
        List<Coordinate2>? toAdd;

        switch( connectionType )
        {
            case RouteConnectionType.StartToStart:
            case RouteConnectionType.EndToEnd:
                toAdd = route2.Points;
                toAdd.Reverse();
                break;

            case RouteConnectionType.StartToEnd:
            case RouteConnectionType.EndToStart:
                toAdd = route2.Points;
                break;

            default:
                // shouldn't ever get here
                throw new InvalidEnumArgumentException(
                    $"Unsupported {typeof( RouteConnectionType )} value '{connectionType}'" );
        }

        switch( connectionType )
        {
            case RouteConnectionType.StartToStart:
            case RouteConnectionType.StartToEnd:
                retVal.Points.InsertRange( 0, toAdd );
                break;

            case RouteConnectionType.EndToStart:
            case RouteConnectionType.EndToEnd:
                retVal.Points.AddRange( toAdd );
                break;
        }

        return retVal;
    }
}
