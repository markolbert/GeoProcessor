#region copyright
// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// MergeRoutes.cs
//
// This file is part of JumpForJoy Software's GeoProcessor.
// 
// GeoProcessor is free software: you can redistribute it and/or modify it 
// under the terms of the GNU General Public License as published by the 
// Free Software Foundation, either version 3 of the License, or 
// (at your option) any later version.
// 
// GeoProcessor is distributed in the hope that it will be useful, but 
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
// or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License 
// for more details.
// 
// You should have received a copy of the GNU General Public License along 
// with GeoProcessor. If not, see <https://www.gnu.org/licenses/>.
#endregion

using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.GeoProcessor;

[BeforeImportFilters(DefaultFilterName, 30)]
public class MergeRoutes : ImportFilter
{
    public const string DefaultFilterName = "Merge Routes";

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

                //var match1 = FoundMatch( filteredInput[ curSet.RouteIndex ], "Having a lunch break" );
                //match1 |= FoundMatch( filteredInput[ adjacent.ConnectedRouteIndex ],"Having a lunch break" );

                //var match2 = FoundMatch(filteredInput[curSet.RouteIndex], "Took a short hike");
                //match2 |= FoundMatch(filteredInput[adjacent.ConnectedRouteIndex], "Took a short hike");

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

    //private bool FoundMatch( IImportedRoute route, string toMatch ) =>
    //    route.Any( x => x.Description?.Contains( toMatch ) ?? false );

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
