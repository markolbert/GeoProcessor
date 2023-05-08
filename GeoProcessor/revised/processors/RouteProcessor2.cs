using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.GeoProcessor;

public abstract class RouteProcessor2 : MessageBasedTask, IRouteProcessor2
{
    protected RouteProcessor2(
        string? mesgPrefix = null,
        ILoggerFactory? loggerFactory = null,
        params IImportFilter[] requiredImportFilters
    )
        : base( mesgPrefix, loggerFactory )
    {
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
    public List<IImportFilter> ImportFilters { get; }
    public string ApiKey { get; set; } = string.Empty;
    public TimeSpan RequestTimeout { get; set; } = GeoConstants.DefaultRequestTimeout;

    public double MaxPointSeparation { get; set; } = GeoConstants.DefaultMaxPointSeparationKm;

    public async Task<ProcessRouteResult> ProcessRoute(
        List<ImportedRoute> toProcess,
        CancellationToken ctx = default
    )
    {
        await OnProcessingStarted();

        foreach( var filter in ImportFilters.Distinct()
                                                .OrderBy( x => x.Category )
                                                .ThenBy( x => x.Priority ) )
        {
            Logger?.LogInformation( "Executing {filter} filter...", filter.FilterName );
            toProcess = filter.Filter( toProcess );
        }

        Logger?.LogInformation( "Filtering complete" );

        var retVal = await ProcessRouteInternalAsync( toProcess, ctx );

        await OnProcessingEnded();

        return retVal;
    }

    protected abstract Task<ProcessRouteResult> ProcessRouteInternalAsync(
        List<ImportedRoute> importedRoutes,
        CancellationToken ctx
    );
}
