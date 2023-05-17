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
using System.IO;
// ReSharper disable NotAccessedPositionalProperty.Local

namespace J4JSoftware.RouteSnapper;

public static partial class GeoExtensions
{
    public static string ChangeFileExtension( string filePath, string extension )
    {
        var dirPath = Path.GetDirectoryName( filePath ) ?? string.Empty;
        var noExt = Path.GetFileNameWithoutExtension( filePath );

        return string.IsNullOrEmpty( extension )
            ? Path.Combine( dirPath, noExt )
            : Path.Combine( dirPath, $"{noExt}.{extension}" );
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