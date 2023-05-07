using System.Collections.Generic;

namespace J4JSoftware.GeoProcessor;

public interface IImportFilter
{
    string FilterName { get; }

    List<ImportedRoute> Filter( List<ImportedRoute> input );
}