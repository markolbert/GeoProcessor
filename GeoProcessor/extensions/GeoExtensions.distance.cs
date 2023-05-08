using System;
using System.Collections.Generic;
using System.Linq;

namespace J4JSoftware.GeoProcessor;

public static partial class GeoExtensions
{
    public static double Convert( this Distance2 distance, UnitType newUnits ) =>
        Convert( distance.Value, distance.Units, newUnits );

    public static double Convert(this double value, UnitType currentUnits, UnitType newUnits)
    {

        if( currentUnits == newUnits )
            return value;

        var curInfo = GetMeasurementInfo(currentUnits);
        var newInfo = GetMeasurementInfo(newUnits);

        var systemFactor = curInfo.System == newInfo.System
            ? 1d
            : curInfo.System == MeasurementSystem.American
                ? 1 / GeoConstants.FeetPerMeter
                : GeoConstants.FeetPerMeter;

        var convertedNormalized = value * curInfo.ScaleFactor * systemFactor;

        return convertedNormalized / newInfo.ScaleFactor;
    }

    public static double GetDistance( double lat1, double long1, double lat2, double long2 )
    {
        var ptPair = new PointPair( new Coordinate2( lat1, long1 ), new Coordinate2( lat2, long2 ) );
        return ptPair.GetDistance( UnitType.Miles ).Value;
    }

    public static Distance GetDistance( Coordinate c1, Coordinate c2 )
    {
        var miles = GetDistance( new PointPair( new Coordinate2( c1.Latitude, c1.Longitude ),
                                                new Coordinate2( c2.Latitude, c2.Longitude ) ),
                                 UnitType.Miles );

        return new Distance( UnitTypes.mi, miles.Value);
    }

    public static Distance2 GetDistance(
        this Coordinate2 start,
        Coordinate2 end,
        UnitType units = UnitType.Kilometers
    ) =>
        GetDistance( new PointPair( start, end ), units );

    public static Distance2 GetDistance( this PointPair pointPair, UnitType units = UnitType.Kilometers )
    {
        var deltaLat = ( pointPair.Second.Latitude - pointPair.First.Latitude ) * GeoConstants.RadiansPerDegree;
        var deltaLong = ( pointPair.Second.Longitude - pointPair.First.Longitude ) * GeoConstants.RadiansPerDegree;

        var h1 = Math.Sin( deltaLat / 2 ) * Math.Sin( deltaLat / 2 )
          + Math.Cos( pointPair.First.Latitude * GeoConstants.RadiansPerDegree )
          * Math.Cos( pointPair.Second.Latitude * GeoConstants.RadiansPerDegree )
          * Math.Sin( deltaLong / 2 )
          * Math.Sin( deltaLong / 2 );

        var h2 = 2 * Math.Asin( Math.Min( 1, Math.Sqrt( h1 ) ) );

        var distance = new Distance2( UnitType.Kilometers, h2 * GeoConstants.EarthRadiusInKilometers );

        return distance.ChangeUnits( units );
    }

    public static double GetBearing( Coordinate c1, Coordinate c2 ) =>
        GetBearing( new PointPair( new Coordinate2( c1.Latitude, c1.Longitude ),
                                   new Coordinate2( c2.Latitude, c2.Longitude ) ) );

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
}
