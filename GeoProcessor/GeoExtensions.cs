#region copyright
// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// GeoExtensions.cs
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Xml.Linq;

namespace J4JSoftware.GeoProcessor;

public static class GeoExtensions
{
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

    public static double GetDistance( double lat1, double long1, double lat2, double long2 )
    {
        var ptPair = new PointPair( new Coordinate2( lat1, long1 ), new Coordinate2( lat2, long2 ) );
        return ptPair.GetDistance();
    }

    public static Distance GetDistance( Coordinate c1, Coordinate c2 )
    {
        var miles = GetDistance( new PointPair( new Coordinate2( c1.Latitude, c1.Longitude ),
                                                new Coordinate2( c2.Latitude, c2.Longitude ) ) );

        return new Distance( UnitTypes.mi, miles );
    }

    public static double GetDistance( this Coordinate2 start, Coordinate2 end, bool inKilometers = true ) =>
        GetDistance( new PointPair( start, end ), inKilometers );

    public static double GetDistance( this PointPair pointPair, bool inKilometers = true )
    {
        var deltaLat = ( pointPair.Second.Latitude - pointPair.First.Latitude ) * GeoConstants.RadiansPerDegree;
        var deltaLong = ( pointPair.Second.Longitude - pointPair.First.Longitude ) * GeoConstants.RadiansPerDegree;

        var h1 = Math.Sin( deltaLat / 2 ) * Math.Sin( deltaLat / 2 )
          + Math.Cos( pointPair.First.Latitude * GeoConstants.RadiansPerDegree )
          * Math.Cos( pointPair.Second.Latitude * GeoConstants.RadiansPerDegree )
          * Math.Sin( deltaLong / 2 )
          * Math.Sin( deltaLong / 2 );

        var h2 = 2 * Math.Asin( Math.Min( 1, Math.Sqrt( h1 ) ) );

        return h2 * ( inKilometers ? GeoConstants.EarthRadiusInKilometers : GeoConstants.EarthRadiusInMiles );
    }

    public static double GetBearing(Coordinate c1, Coordinate c2) =>
        GetBearing(new PointPair(new Coordinate2(c1.Latitude, c1.Longitude),
                                  new Coordinate2(c2.Latitude, c2.Longitude)));

    public static double GetBearing( this PointPair pointPair )
    {
        var deltaLongitude = ( pointPair.Second.Longitude - pointPair.First.Longitude ) * GeoConstants.RadiansPerDegree;

        var y = Math.Sin( deltaLongitude ) * Math.Cos( pointPair.Second.Latitude * GeoConstants.RadiansPerDegree );

        var x = Math.Cos( pointPair.First.Latitude * GeoConstants.RadiansPerDegree )
          * Math.Sin( pointPair.Second.Latitude * GeoConstants.RadiansPerDegree )
          - Math.Sin( pointPair.First.Latitude * GeoConstants.RadiansPerDegree )
          * Math.Cos( pointPair.Second.Latitude * GeoConstants.RadiansPerDegree )
          * Math.Cos( deltaLongitude );

        var theta = Math.Atan2( y, x );

        return ( theta * GeoConstants.DegreesPerRadian + 360 ) % 360;
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

    public static bool IsNamedElement( this XElement element, string elementName ) =>
        element.Name.LocalName.Equals( elementName, StringComparison.OrdinalIgnoreCase );

    public static string? GetFirstDescendantValue( this XElement element, string elementName ) =>
        element.Descendants()
               .FirstOrDefault( x => x.Name.LocalName.Equals( elementName, StringComparison.OrdinalIgnoreCase ) )
              ?.Value;

    public static List<XElement> GetNamedDescendants( this XElement element, string descendantName ) =>
        element.Descendants()
               .Where( x => x.Name.LocalName.Equals( descendantName, StringComparison.OrdinalIgnoreCase ) )
               .ToList();

    public static IEnumerable<XElement> GetNamedDescendants( this List<XElement> element, string descendantName ) =>
        element.SelectMany( x => x.Descendants()
                                  .Where( y => y.Name.LocalName.Equals( descendantName,
                                                                        StringComparison.OrdinalIgnoreCase ) ) );

    public static bool TryParseAttribute<T>( this XElement element, string attributeName, out T value )
    where T : struct
    {
        value = default;

        var text = element.Attributes()
                          .FirstOrDefault(
                               x => x.Name.LocalName.Equals( attributeName, StringComparison.OrdinalIgnoreCase ) )
                         ?.Value;

        if( string.IsNullOrEmpty( text ) )
            return false;

        try
        {
            value = (T) Convert.ChangeType( text, typeof( T ) );
        }
        catch
        {
            return false;
        }

        return true;
    }

    public static bool TryParseFirstDescendantValue<T>(this XElement parent, string descendentName, out T value)
        where T : struct
    {
        value = default;

        var text = parent.GetFirstDescendantValue(descendentName);

        if (string.IsNullOrEmpty(text))
            return false;

        try
        {
            value = (T)Convert.ChangeType(text, typeof(T));
        }
        catch
        {
            return false;
        }

        return true;
    }

    public static Coordinate2 Start( this ImportedRoute route ) =>
        route.Points.FirstOrDefault() ?? new Coordinate2( 0, 0 );

    public static Coordinate2 End(this ImportedRoute route) =>
        route.Points.LastOrDefault() ?? new Coordinate2(0, 0);

    public static double StartToStart( this ImportedRoute route1, ImportedRoute route2, bool inKilometers = true ) =>
        new PointPair( route1.Start(), route2.Start() ).GetDistance( inKilometers );

    public static double StartToEnd(this ImportedRoute route1, ImportedRoute route2, bool inKilometers = true) =>
        new PointPair(route1.Start(), route2.End()).GetDistance(inKilometers);

    public static double EndToStart(this ImportedRoute route1, ImportedRoute route2, bool inKilometers = true) =>
        new PointPair(route1.End(), route2.Start()).GetDistance(inKilometers);

    public static double EndToEnd(this ImportedRoute route1, ImportedRoute route2, bool inKilometers = true) =>
        new PointPair(route1.End(), route2.End()).GetDistance(inKilometers);

}