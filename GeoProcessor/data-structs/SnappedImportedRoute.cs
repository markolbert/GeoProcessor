#region copyright
// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// SnappedImportedRoute.cs
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

public class SnappedImportedRoute : IImportedRouteChunk
{
    public SnappedImportedRoute(
        ImportedRouteChunk unprocessedRouteChunk,
        List<Coordinate2> processedPoints
    )
    {
        SourceRoute = unprocessedRouteChunk;
        ProcessedPoints = processedPoints;
    }

    public int NumPoints => ProcessedPoints.Count;
    public string? RouteName => SourceRoute.RouteName;
    public string? Description => SourceRoute.Description;
    public int RouteId => ((ImportedRouteChunk)SourceRoute).RouteId;
    public int ChunkSize => ((ImportedRouteChunk)SourceRoute).ChunkSize;
    public int ChunkNum => ((ImportedRouteChunk) SourceRoute).ChunkNum;
    public IImportedRoute SourceRoute { get; }
    public List<Coordinate2> ProcessedPoints { get; }

    public IEnumerator<Coordinate2> GetEnumerator() => ProcessedPoints.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

}
