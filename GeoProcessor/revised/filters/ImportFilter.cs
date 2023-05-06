using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.GeoProcessor;

public abstract class ImportFilter : IImportFilter
{
    #region IEqualityComparer

    public bool Equals(IImportFilter? x, IImportFilter? y)
    {
        if (ReferenceEquals(x, y))
            return true;
        if (ReferenceEquals(x, null))
            return false;
        if (ReferenceEquals(y, null))
            return false;
        if (x.GetType() != y.GetType())
            return false;

        return string.Equals(x.FilterName, y.FilterName, StringComparison.OrdinalIgnoreCase);
    }

    public int GetHashCode(IImportFilter obj)
    {
        return StringComparer.OrdinalIgnoreCase.GetHashCode(obj.FilterName);
    }

    #endregion

    protected ImportFilter(
        ILoggerFactory? loggerFactory
    )
    {
        Logger = loggerFactory?.CreateLogger( GetType() );

        var type = GetType();
        var attr = type.GetCustomAttribute<ImportFilterAttribute>();

        if (string.IsNullOrEmpty(attr?.FilterName))
        {
            Logger?.LogCritical("Import filter {type} not decorated with a valid {attr}",
                                type,
                                typeof(ImportFilterAttribute));

            throw new NullReferenceException(
                $"Route processor {type} not decorated with a valid {typeof(ImportFilterAttribute)}");
        }

        FilterName = attr.FilterName;
    }

    protected ILogger? Logger { get; }

    public string FilterName { get; }

    public abstract List<ImportedRoute> Filter( List<ImportedRoute> input );
}
