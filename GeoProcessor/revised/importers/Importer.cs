using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using J4JSoftware.GeoProcessor.RouteBuilder;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.GeoProcessor;

public abstract class Importer : MessageBasedTask, IImporter2
{
    private double _maxPtSep = GeoConstants.DefaultMaxPointSeparationKm;

    protected Importer(
        string? mesgPrefix = null,
        ILoggerFactory? loggerFactory = null
    )
    : base(mesgPrefix, loggerFactory)
    {
    }

    // in kilometers
    public double MaxPointSeparation
    {
        get => _maxPtSep;
        set => _maxPtSep = value <= 0 ? GeoConstants.DefaultMaxPointSeparationKm : value;
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
