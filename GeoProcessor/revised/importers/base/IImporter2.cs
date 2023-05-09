using J4JSoftware.GeoProcessor.RouteBuilder;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace J4JSoftware.GeoProcessor;

public interface IImporter2 : IMessageBasedTask
{
    Task<List<ImportedRoute>> ImportAsync(DataToImportBase toImport, CancellationToken ctx = default);
}
