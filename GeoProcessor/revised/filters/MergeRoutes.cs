using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.GeoProcessor;

[BeforeAllImportFilter("Merge Routes", 1)]
public class MergeRoutes : ImportFilter
{
    private Distance2 _maxRouteGap = new( UnitType.Meters, GeoConstants.DefaultMaxRouteGapMeters );

    public MergeRoutes(
        ILoggerFactory? loggerFactory
    )
    :base( loggerFactory)
    {
    }

    public Distance2 MaximumRouteGap
    {
        get => _maxRouteGap;

        set =>
            _maxRouteGap = value.Value <= 0
                ? new Distance2( UnitType.Meters, GeoConstants.DefaultMaxRouteGapMeters )
                : value;
    }

    public override List<IImportedRoute> Filter( List<IImportedRoute> input )
    {
        if( input.Count <= 1 )
            return input;

        var filteredInput = input.Where( x => x.NumPoints > 1 )
                                 .ToList();

        if( filteredInput.Count != input.Count )
            Logger?.LogTrace( "Ignoring routes with less than 2 points" );

        var retVal = new List<IImportedRoute>();

        var connections = GetConnections(filteredInput);
        var prevConnections = 0;
        var curConnections = connections.Count;

        while ( filteredInput.Any() && prevConnections != curConnections )
        {
            var curSet = connections.First();

            var adjacentRoutes = curSet.GetClosest( MaximumRouteGap );

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
                var mergedRoute = new MergedImportedRoute( filteredInput[ curSet.RouteIndex ],
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

    private static List<RouteConnections> GetConnections( List<IImportedRoute> routes )
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
                                                                .StartToStart( routes[ innerIdx ] ) ) );

                curSet.Connections.Add( new RouteConnection( innerIdx,
                                                             RouteConnectionType.StartToEnd,
                                                             routes[ outerIdx ]
                                                                .StartToEnd( routes[ innerIdx ] ) ) );

                curSet.Connections.Add( new RouteConnection( innerIdx,
                                                             RouteConnectionType.EndToStart,
                                                             routes[ outerIdx ].EndToStart( routes[ innerIdx ] ) ) );

                curSet.Connections.Add( new RouteConnection( innerIdx,
                                                             RouteConnectionType.EndToEnd,
                                                             routes[ outerIdx ].EndToEnd( routes[ innerIdx ] ) ) );
            }

            retVal.Add( curSet );
        }

        return retVal;
    }
}
