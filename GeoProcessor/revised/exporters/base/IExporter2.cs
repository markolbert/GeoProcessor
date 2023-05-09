using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace J4JSoftware.GeoProcessor;

public interface IExporter2 : IMessageBasedTask
{
    Task<bool> ExportAsync( IEnumerable<ImportedRoute> routes, CancellationToken ctx = default );
}