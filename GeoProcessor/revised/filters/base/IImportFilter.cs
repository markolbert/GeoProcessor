using System.Collections.Generic;

namespace J4JSoftware.GeoProcessor;

public interface IImportFilter
{
    string FilterName { get; }
    ImportFilterCategory Category { get; }
    uint Priority { get; }

    List<ImportedRoute> Filter( List<ImportedRoute> input );
}