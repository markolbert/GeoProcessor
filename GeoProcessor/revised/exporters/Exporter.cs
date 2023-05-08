using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.GeoProcessor;

public abstract class Exporter : IExporter2
{
    protected Exporter(
        ILoggerFactory? loggerFactory
    )
    {
        Logger = loggerFactory?.CreateLogger( GetType() );
    }

    protected ILogger? Logger { get; }

    public abstract Task<bool> ExportAsync( IEnumerable<ExportedRoute> routes, CancellationToken ctx = default );

    public Func<StatusInformation, Task>? StatusReporter { get; set; }
    public Func<ProcessingMessage, Task>? MessageReporter { get; set; }
    public int StatusInterval { get; set; }
}