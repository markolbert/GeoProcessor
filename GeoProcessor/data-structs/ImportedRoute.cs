#region copyright
// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// ImportedRoute.cs
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

using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace J4JSoftware.GeoProcessor;

public class ImportedRoute : IImportedRoute
{
    public ImportedRoute()
    {
        Points = new Points();
    }

    public ImportedRoute(
        Points points
    )
    {
        Points = points;
    }

    public ImportedRoute Copy() => new( new Points( Points ) ) { RouteName = RouteName };

    public string? RouteName { get; set; }
    public string? Description { get; set; }

    public int NumPoints(Distance? minSeparation = null, Distance? maxOverallGap = null) =>
        Points.GetFilteredPoints(minSeparation, maxOverallGap).Count();

    public Points Points { get; }

    public bool Any(Distance? minSeparation = null, Distance? maxOverallGap = null) =>
        Points.Any(minSeparation, maxOverallGap);

    public IEnumerable<Point> GetFilteredPoints(Distance? minSeparation = null, Distance? maxOverallGap = null) =>
        Points.GetFilteredPoints(minSeparation, maxOverallGap);
}