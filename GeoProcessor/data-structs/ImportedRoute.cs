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

namespace J4JSoftware.GeoProcessor;

public class ImportedRoute : IImportedRoute
{
    public ImportedRoute()
    {
        Points = new List<Coordinates>();
    }

    public ImportedRoute(
        List<Coordinates> points
    )
    {
        Points = points;
    }

    public ImportedRoute Copy() => new( new List<Coordinates>( Points ) ) { RouteName = RouteName };

    public string? RouteName { get; set; }
    public string? Description { get; set; }

    public int NumPoints => Points.Count;
    public List<Coordinates> Points { get; }

    public IEnumerator<Coordinates> GetEnumerator() => ( (IEnumerable<Coordinates>) Points ).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}