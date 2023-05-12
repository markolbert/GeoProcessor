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
