#region copyright
// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// GeoExtensions.routes.cs
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

using System.Collections.Generic;
using System.Linq;

namespace J4JSoftware.RouteSnapper;

public static partial class GeoExtensions
{
    public static Point Start( this Route route ) => route.Points.First();
    public static Point End( this Route route ) => route.Points.Last();

    public static Distance StartToStart( this Route route1, Route route2 ) =>
        new PointPair( route1.Start(), route2.Start() ).GetDistance();

    public static Distance StartToEnd( this Route route1, Route route2 ) =>
        new PointPair( route1.Start(), route2.End() ).GetDistance();

    public static Distance EndToStart( this Route route1, Route route2) =>
        new PointPair( route1.End(), route2.Start() ).GetDistance();

    public static Distance EndToEnd( this Route route1, Route route2 ) =>
        new PointPair( route1.End(), route2.End() ).GetDistance();
}
