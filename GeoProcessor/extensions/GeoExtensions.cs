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

using System.Drawing;
using System.Reflection;
// ReSharper disable NotAccessedPositionalProperty.Local

namespace J4JSoftware.GeoProcessor;

public static partial class GeoExtensions
{
    private record MeasurementInfo( MeasurementSystem System, double ScaleFactor );

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

    public static Color RouteColorPicker( SnappedRoute route, int routeIndex )
    {
        routeIndex = routeIndex % 10;

        return routeIndex switch
        {
            0 => Color.Blue,
            1 => Color.Green,
            2 => Color.Red,
            3 => Color.Yellow,
            4 => Color.Purple,
            5 => Color.Orange,
            6 => Color.Aqua,
            7 => Color.MediumSpringGreen,
            8 => Color.NavajoWhite,
            _ => Color.Fuchsia
        };
    }

    public static int RouteWidthPicker( SnappedRoute route, int routeIndex ) => GeoConstants.DefaultRouteWidth;
}