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
using System.Reflection;

namespace J4JSoftware.GeoProcessor;

public static partial class GeoExtensions
{
    private record MeasurementInfo( MeasurementSystem System, double ScaleFactor );

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

    public static BingMapsRESTToolkit.Coordinate ToBingMapsCoordinate( this Coordinate coordinate )
    {
        return new BingMapsRESTToolkit.Coordinate
        {
            Latitude = coordinate.Latitude,
            Longitude = coordinate.Longitude
        };
    }

    private static MeasurementInfo GetMeasurementInfo( UnitType unitType )
    {
        var curField = typeof( UnitType ).GetField( unitType.ToString() );

        MeasurementInfo retVal;

        if (curField == null)
            retVal = new MeasurementInfo( MeasurementSystem.Metric, 1 );
        else
        {
            var attr = curField.GetCustomAttribute<MeasurementSystemAttribute>();

            retVal = attr == null
                ? new MeasurementInfo( MeasurementSystem.Metric, 1 )
                : new MeasurementInfo( attr.MeasurementSystem, attr.ScaleFactor );
        }

        return retVal;
    }
}