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
