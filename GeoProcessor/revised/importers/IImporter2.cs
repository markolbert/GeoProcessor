using J4JSoftware.GeoProcessor.RouteBuilder;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace J4JSoftware.GeoProcessor;

public interface IImporter2 : IMessageBasedTask
{
    // in kilometers
    double MaxPointSeparation { get; set; }

    Task<List<ImportedRoute>> ImportAsync(DataToImportBase toImport, CancellationToken ctx = default);
}
