using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

namespace J4JSoftware.GeoProcessor;

public interface IExporter2 : IMessageBasedTask
{
    ReadOnlyCollection<IImportFilter> ImportFilters { get; }
    void ClearImportFilters();
    bool AddFilter( IImportFilter filter );
    bool AddFilters( IEnumerable<IImportFilter> filters );

    Task<bool> ExportAsync( List<ImportedRoute> routes, CancellationToken ctx = default );
}