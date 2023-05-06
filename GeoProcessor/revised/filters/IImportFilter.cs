using System.Collections.Generic;

namespace J4JSoftware.GeoProcessor;

public interface IImportFilter : IEqualityComparer<IImportFilter>
{
    string FilterName { get; }

    List<ImportedRoute> Filter( List<ImportedRoute> input );
}