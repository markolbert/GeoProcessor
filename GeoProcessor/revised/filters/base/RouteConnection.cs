using System;
using System.Collections.Generic;

namespace J4JSoftware.GeoProcessor;

public record RouteConnection( int ConnectedRouteIndex, RouteConnectionType Type, double Gap );

public record RouteConnections( int RouteIndex, List<RouteConnection> Connections )
{
    public List<RouteConnection> GetClosest( double maxGap, double equalityTolerance )
    {
        var retVal = new List<RouteConnection>();

        var minGap = double.MaxValue;

        foreach( var connection in Connections )
        {
            if( connection.Gap > maxGap )
                continue;

            if( Math.Abs( connection.Gap - minGap ) < equalityTolerance )
                retVal.Add( connection );
            else
            {
                if( connection.Gap > minGap )
                    continue;

                minGap = connection.Gap;

                retVal.Clear();
                retVal.Add( connection );
            }
        }

        return retVal;
    }
}
