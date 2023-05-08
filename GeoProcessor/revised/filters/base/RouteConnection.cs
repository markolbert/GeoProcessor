using System.Collections.Generic;

namespace J4JSoftware.GeoProcessor;

public record RouteConnection( int ConnectedRouteIndex, RouteConnectionType Type, Distance2 Gap );

public record RouteConnections( int RouteIndex, List<RouteConnection> Connections )
{
    public List<RouteConnection> GetClosest( Distance2 maxGap )
    {
        var retVal = new List<RouteConnection>();

        var minGap = maxGap with { Value = double.MaxValue };

        foreach( var connection in Connections )
        {
            if( connection.Gap > maxGap )
                continue;

            if( connection.Gap == minGap)
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
