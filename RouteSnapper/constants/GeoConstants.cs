#region copyright
// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// GeoConstants.cs
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
using System.Drawing;

namespace J4JSoftware.RouteSnapper;

public partial class GeoConstants
{
    public static TimeSpan DefaultRequestTimeout { get; } = TimeSpan.FromSeconds(20);
    public const int DefaultProgressInterval = 500;
    public static Color DefaultRouteColor { get; }= Color.Blue;
    public static int DefaultRouteWidth = 10;
    public const string DefaultIconSourceHref = "http://maps.google.com/mapfiles/kml/paddle/wht-blank.png";
}
