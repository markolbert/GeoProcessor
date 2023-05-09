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

    public IEnumerator<Coordinate2> GetEnumerator() =>
        SourceRoute.Skip( ChunkNum * ChunkSize )
                    .Take( ChunkSize ).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}