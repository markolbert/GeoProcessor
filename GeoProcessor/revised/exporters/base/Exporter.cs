using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.GeoProcessor;

public abstract class Exporter : MessageBasedTask, IExporter2
{
    private readonly List<IImportFilter> _importFilters = new();

    protected Exporter(
        ILoggerFactory? loggerFactory
    )
        : base( null, loggerFactory )
    {
    }

    public ReadOnlyCollection<IImportFilter> ImportFilters => _importFilters.AsReadOnly();

    public void ClearImportFilters() => _importFilters.Clear();

    public bool AddFilter( IImportFilter filter )
    {
        if( filter.Category != ImportFilterCategory.PostSnapping )
        {
            Logger?.LogError( "Filter '{name}' is not a {snapping} filter",
                              filter.FilterName,
                              typeof( ImportFilterCategory ) );
            return false;
        }

        _importFilters.Add( filter );
        return true;
    }

    public bool AddFilters( IEnumerable<IImportFilter> filters ) =>
        filters.Aggregate( true, ( current, filter ) => current & AddFilter( filter ) );

    public async Task<bool> ExportAsync( List<ImportedRoute> routes, CancellationToken ctx = default )
    {
        var filters = AdjustImportFilters();
        var toProcess = routes.Cast<IImportedRoute>().ToList();

        foreach( var filter in filters.Where( x => x.Category == ImportFilterCategory.PostSnapping )
                                      .OrderBy( x => x.Category )
                                      .ThenBy( x => x.Priority ) )
        {
            Logger?.LogInformation( "Executing {filter} filter...", filter.FilterName );
            toProcess = filter.Filter( toProcess );
        }

        Logger?.LogInformation("Filtering complete");

        return await ExportInternalAsync( toProcess, ctx );
    }

    protected virtual List<IImportFilter> AdjustImportFilters()=> _importFilters;

    protected abstract Task<bool> ExportInternalAsync( List<IImportedRoute> routes, CancellationToken ctx );
}