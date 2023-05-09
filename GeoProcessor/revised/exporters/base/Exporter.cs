using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.GeoProcessor;

public abstract class Exporter : MessageBasedTask, IExporter2
{
    protected Exporter(
        ILoggerFactory? loggerFactory
    )
        : base( null, loggerFactory )
    {
    }

    public abstract Task<bool> ExportAsync( IEnumerable<ImportedRoute> routes, CancellationToken ctx = default );
}