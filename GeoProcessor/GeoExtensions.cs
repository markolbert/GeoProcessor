#region license

// Copyright 2021 Mark A. Olbert
// 
// This library or program 'GeoProcessor' is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public License as
// published by the Free Software Foundation, either version 3 of the License,
// or (at your option) any later version.
// 
// This library or program is distributed in the hope that it will be useful, but
// WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// General Public License for more details.
// 
// You should have received a copy of the GNU General Public License along with
// this library or program.  If not, see <https://www.gnu.org/licenses/>.

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace J4JSoftware.GeoProcessor;

public static class GeoExtensions
{
    public const double RadiansPerDegree = Math.PI / 180;
    public const double DegreesPerRadian = 180 / Math.PI;

    public static bool SnapsToRoute( this ProcessorType procType )
    {
        var memInfo = typeof( ProcessorType ).GetField( procType.ToString() );
        if( memInfo == null )
            return false;

        return memInfo.GetCustomAttribute<ProcessorTypeInfoAttribute>()?.IsSnapToRoute ?? false;
    }

    public static bool RequiresApiKey( this ProcessorType procType )
    {
        var memInfo = typeof( ProcessorType ).GetField( procType.ToString() );
        if( memInfo == null )
            return false;

        return memInfo.GetCustomAttribute<ProcessorTypeInfoAttribute>()?.RequiresAPIKey ?? false;
    }

    public static int MaxPointsPerRequest( this ProcessorType procType )
    {
        var memInfo = typeof( ProcessorType ).GetField( procType.ToString() );
        if( memInfo == null )
            return 100;

        return memInfo.GetCustomAttribute<ProcessorTypeInfoAttribute>()?.MaxPointsPerRequest ?? 100;
    }

    public static TAttr? GetTargetType<THandler, TAttr>()
        where TAttr : Attribute
    {
        return GetTargetType<TAttr>( typeof( THandler ) );
    }

    public static TAttr? GetTargetType<TAttr>( Type handlerType )
        where TAttr : Attribute
    {
        return handlerType.GetCustomAttribute<TAttr>();
    }

    public static Distance GetDistance( Coordinate c1, Coordinate c2 )
    {
        var deltaLat = ( c2.Latitude - c1.Latitude ) * RadiansPerDegree;
        var deltaLong = ( c2.Longitude - c1.Longitude ) * RadiansPerDegree;

        var h1 = Math.Sin( deltaLat / 2 ) * Math.Sin( deltaLat / 2 )
          + Math.Cos( c1.Latitude * RadiansPerDegree )
          * Math.Cos( c2.Latitude * RadiansPerDegree )
          * Math.Sin( deltaLong / 2 )
          * Math.Sin( deltaLong / 2 );

        var h2 = 2 * Math.Asin( Math.Min( 1, Math.Sqrt( h1 ) ) );

        return new Distance( UnitTypes.mi, h2 * 3958.8 );
    }

    public static double GetBearing( Coordinate c1, Coordinate c2 )
    {
        var deltaLongitude = ( c2.Longitude - c1.Longitude ) * RadiansPerDegree;

        var y = Math.Sin( deltaLongitude ) * Math.Cos( c2.Latitude * RadiansPerDegree );

        var x = Math.Cos( c1.Latitude * RadiansPerDegree ) * Math.Sin( c2.Latitude * RadiansPerDegree )
          - Math.Sin( c1.Latitude * RadiansPerDegree )
          * Math.Cos( c2.Latitude * RadiansPerDegree )
          * Math.Cos( deltaLongitude );

        var theta = Math.Atan2( y, x );

        return ( theta * DegreesPerRadian + 360 ) % 360;
    }

    public static (double avg, double stdev) GetBearingStatistics(
        this LinkedListNode<Coordinate> startNode,
        LinkedListNode<Coordinate> endNode
    )
    {
        var curNode = startNode;

        var bearings = new List<double>();

        while( curNode != endNode )
        {
            bearings.Add( GetBearing( curNode!.Value, curNode.Next!.Value ) );

            curNode = curNode.Next;
        }

        return ( bearings.Average(), GetStandardDeviation( bearings ) );
    }

    private static double GetStandardDeviation( List<double> values )
    {
        if( values.Count == 0 )
            return 0.0;

        var avg = values.Average();
        var sum = values.Sum( d => ( d - avg ) * ( d - avg ) );

        return Math.Sqrt( sum / values.Count );
    }

    public static BingMapsRESTToolkit.Coordinate ToBingMapsCoordinate( this Coordinate coordinate )
    {
        return new BingMapsRESTToolkit.Coordinate
        {
            Latitude = coordinate.Latitude,
            Longitude = coordinate.Longitude
        };
    }
}