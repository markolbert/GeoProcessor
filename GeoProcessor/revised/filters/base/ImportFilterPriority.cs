using System;
using System.Collections.Generic;

namespace J4JSoftware.GeoProcessor;

public record ImportFilterPriority(
    string FilterName,
    IImportFilter FilterAgent,
    ImportFilterCategory Category,
    uint Priority
) : IEqualityComparer<ImportFilterPriority>
{
    public bool Equals( ImportFilterPriority? x, ImportFilterPriority? y )
    {
        if( ReferenceEquals( x, y ) )
            return true;
        if( ReferenceEquals( x, null ) )
            return false;
        if( ReferenceEquals( y, null ) )
            return false;
        if( x.GetType() != y.GetType() )
            return false;

        return string.Equals( x.FilterName, y.FilterName, StringComparison.OrdinalIgnoreCase );
    }

    public int GetHashCode( ImportFilterPriority obj )
    {
        return StringComparer.OrdinalIgnoreCase.GetHashCode( obj.FilterName );
    }
}