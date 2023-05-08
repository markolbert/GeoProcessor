using System.Collections.Generic;

namespace J4JSoftware.GeoProcessor;

public interface IImportFilter
{
    string FilterName { get; }
    ImportFilterCategory Category { get; }
    uint Priority { get; }

    List<IImportedRoute> Filter( List<IImportedRoute> input );
}