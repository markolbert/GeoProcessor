namespace J4JSoftware.GeoProcessor;

public interface IImportedRouteChunk : IImportedRoute
{
    int RouteId { get; }
    int ChunkSize { get; }
    int ChunkNum { get; }
    IImportedRoute SourceRoute { get; }
}
