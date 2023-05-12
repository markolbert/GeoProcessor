#region copyright
// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// ImportedRouteChunk.cs
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
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace J4JSoftware.GeoProcessor;

public class ImportedRouteChunk : IImportedRouteChunk
{
    public ImportedRouteChunk( 
        IImportedRoute route,
        int routeId,
        int chunkSize,
        int chunkNum 
        )
    {
        SourceRoute = route;
        RouteId = routeId;
        ChunkSize = chunkSize;
        ChunkNum = chunkNum;
    }

    public int RouteId { get; }
    public int ChunkSize { get; }
    public int ChunkNum { get; }
    public IImportedRoute SourceRoute { get; }

    public string? RouteName
    {
        get => $"{SourceRoute.RouteName} chunk {ChunkNum}";
        set => throw new InvalidOperationException( $"Can't set the route name on a {typeof( ImportedRouteChunk )}" );
    }

    public string? Description
    {
        get => $"{SourceRoute.Description} chunk {ChunkNum}";
        set => throw new InvalidOperationException($"Can't set the description on a {typeof(ImportedRouteChunk)}");
    }

    public int NumPoints => SourceRoute.Count();

    public IEnumerator<Coordinates> GetEnumerator() =>
        SourceRoute.Skip( ChunkNum * ChunkSize )
                    .Take( ChunkSize ).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}