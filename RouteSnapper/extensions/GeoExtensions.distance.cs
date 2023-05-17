#region copyright
// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// GeoExtensions.distance.cs
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
using System.Reflection;

namespace J4JSoftware.RouteSnapper;

public static partial class GeoExtensions
{
    private record MeasurementInfo(MeasurementSystem System, double ScaleFactor);

    public static double Convert( this Distance distance, UnitType newUnits ) =>
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

    private static MeasurementInfo GetMeasurementInfo(UnitType unitType)
    {
        var curField = typeof(UnitType).GetField(unitType.ToString());

        MeasurementInfo retVal;

        if (curField == null)
            retVal = new MeasurementInfo(MeasurementSystem.Metric, 1);
        else
        {
            var attr = curField.GetCustomAttribute<MeasurementSystemAttribute>();

            retVal = attr == null
                ? new MeasurementInfo(MeasurementSystem.Metric, 1)
                : new MeasurementInfo(attr.MeasurementSystem, attr.ScaleFactor);
        }

        return retVal;
    }

    public static double GetDistance( double lat1, double long1, double lat2, double long2 )
    {
        var ptPair = new PointPair( new Point { Latitude = lat1, Longitude = long1 },
                                    new Point { Latitude = lat2, Longitude = long2 } );

        return ptPair.GetDistance( UnitType.Miles ).Value;
    }

    public static Distance GetDistance(
        this Point start,
        Point end,
        UnitType units = UnitType.Kilometers
    ) =>
        GetDistance( new PointPair( start, end ), units );

    public static Distance GetDistance( this PointPair pointPair, UnitType units = UnitType.Kilometers )
    {
        var deltaLat = ( pointPair.Second.Latitude - pointPair.First.Latitude ) * GeoConstants.RadiansPerDegree;
        var deltaLong = ( pointPair.Second.Longitude - pointPair.First.Longitude ) * GeoConstants.RadiansPerDegree;

        var h1 = Math.Sin( deltaLat / 2 ) * Math.Sin( deltaLat / 2 )
          + Math.Cos( pointPair.First.Latitude * GeoConstants.RadiansPerDegree )
          * Math.Cos( pointPair.Second.Latitude * GeoConstants.RadiansPerDegree )
          * Math.Sin( deltaLong / 2 )
          * Math.Sin( deltaLong / 2 );

        var h2 = 2 * Math.Asin( Math.Min( 1, Math.Sqrt( h1 ) ) );

        var distance = new Distance( UnitType.Kilometers, h2 * GeoConstants.EarthRadiusInKilometers );

        return distance.ChangeUnits( units );
    }

    public static double GetBearing( this PointPair pointPair, bool absolute = false )
    {
        var deltaLongitude = ( pointPair.Second.Longitude - pointPair.First.Longitude ) * GeoConstants.RadiansPerDegree;

        var y = Math.Sin( deltaLongitude ) * Math.Cos( pointPair.Second.Latitude * GeoConstants.RadiansPerDegree );

        var x = Math.Cos( pointPair.First.Latitude * GeoConstants.RadiansPerDegree )
          * Math.Sin( pointPair.Second.Latitude * GeoConstants.RadiansPerDegree )
          - Math.Sin( pointPair.First.Latitude * GeoConstants.RadiansPerDegree )
          * Math.Cos( pointPair.Second.Latitude * GeoConstants.RadiansPerDegree )
          * Math.Cos( deltaLongitude );

        var theta = Math.Atan2( y, x );

        return absolute
            ? Math.Abs( ( theta * GeoConstants.DegreesPerRadian + 360 ) % 360 )
            : ( theta * GeoConstants.DegreesPerRadian + 360 ) % 360;
    }
}
