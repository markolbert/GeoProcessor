using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using J4JSoftware.GeoProcessor.RouteBuilder;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.GeoProcessor;

public abstract class Importer : MessageBasedTask, IImporter2
{
    protected Importer(
        string? mesgPrefix = null,
        ILoggerFactory? loggerFactory = null
    )
    : base(mesgPrefix, loggerFactory)
    {
    }

    public async Task<List<ImportedRoute>> ImportAsync( DataToImportBase toImport, CancellationToken ctx = default )
    {
        await OnProcessingStarted();

        var retVal = await ImportInternalAsync( toImport, ctx );

        await OnProcessingEnded();

        return retVal;
    }

    protected abstract Task<List<ImportedRoute>> ImportInternalAsync( DataToImportBase toImport, CancellationToken ctx );
}
