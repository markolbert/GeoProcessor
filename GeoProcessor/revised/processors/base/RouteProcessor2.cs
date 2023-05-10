using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.GeoProcessor;

public abstract class RouteProcessor2 : MessageBasedTask, IRouteProcessor2
{
    protected RouteProcessor2(
        int maxPtsPerRequest,
        string? mesgPrefix = null,
        ILoggerFactory? loggerFactory = null,
        params IImportFilter[] requiredImportFilters
    )
        : base( mesgPrefix, loggerFactory )
    {
        MaxPointsInRequest = maxPtsPerRequest;
        ImportFilters = requiredImportFilters.ToList();

        var type = GetType();
        var procAttr = type.GetCustomAttribute<RouteProcessorAttribute2>();

        if( string.IsNullOrEmpty( procAttr?.Processor ) )
        {
            Logger?.LogCritical( "Route processor {type} not decorated with a valid {attr}",
                                 type,
                                 typeof( RouteProcessorAttribute2 ) );

            throw new NullReferenceException(
                $"Route processor {type} not decorated with a valid {typeof( RouteProcessorAttribute2 )}" );
        }

        ProcessorName = procAttr.Processor;
    }

    public string ProcessorName { get; }
    public List<IImportFilter> ImportFilters { get; private set; }
    public string ApiKey { get; set; } = string.Empty;
    public TimeSpan RequestTimeout { get; set; } = GeoConstants.DefaultRequestTimeout;

    public int MaxPointsInRequest { get; }

    public async Task<List<ImportedRoute>> ProcessRoute(
        List<IImportedRoute> toProcess,
        CancellationToken ctx = default
    )
    {
        await OnProcessingStarted();

        ImportFilters = AdjustImportFilters();

        foreach( var filter in ImportFilters.Where( x => x.Category != ImportFilterCategory.PostSnapping )
                                            .OrderBy( x => x.Category )
                                            .ThenBy( x => x.Priority ) )
        {
            Logger?.LogInformation( "Executing {filter} filter...", filter.FilterName );
            toProcess = filter.Filter( toProcess );
        }

        Logger?.LogInformation("Filtering complete");

        var routeChunks = GetRouteChunks( toProcess );
        var processedChunks = await ProcessRouteChunksAsync(routeChunks, ctx);

        await OnProcessingEnded();

        return processedChunks.MergeProcessedRouteChunks( Logger );
    }

    protected virtual List<IImportFilter> AdjustImportFilters()
    {
        return ImportFilters.Distinct().ToList();
    }

    private List<ImportedRouteChunk> GetRouteChunks( List<IImportedRoute> routes )
    {
        var retVal = new List<ImportedRouteChunk>();

        // split route into chunks if point count exceeds chunk size
        for( var idx = 0; idx < routes.Count; idx++ )
        {
            var route = routes[ idx ];

            var numChunks = MaxPointsInRequest <= 0
                ? 1
                : (int) Math.Ceiling( route.NumPoints / (double) MaxPointsInRequest );

            if( numChunks == 1 )
                retVal.Add( new ImportedRouteChunk( route, idx, MaxPointsInRequest, 0 ) );
            else
            {
                for( var chunkNum = 0; chunkNum < numChunks; chunkNum++ )
                {
                    retVal.Add( new ImportedRouteChunk( route, idx, MaxPointsInRequest, chunkNum ) );
                }
            }
        }

        return retVal;
    }

    protected abstract Task<List<SnappedImportedRoute>> ProcessRouteChunksAsync(
        List<ImportedRouteChunk> routeChunks,
        CancellationToken ctx
    );
}
