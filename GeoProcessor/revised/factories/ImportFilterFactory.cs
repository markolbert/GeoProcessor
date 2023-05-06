using System;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.GeoProcessor;

public class ImportFilterFactory : NamedTypeFactory<IImportFilter, ImportFilterAttribute>
{
    private readonly ILoggerFactory? _loggerFactory;

    public ImportFilterFactory(
        ILoggerFactory? loggerFactory = null
    )
        : base( x=>x.FilterName, loggerFactory )
    {
        _loggerFactory = loggerFactory;
    }

    protected override bool CheckConstructor( ConstructorInfo ctor )
    {
        var ctorArgs = ctor.GetParameters();

        return ctorArgs.Length == 1 && ctorArgs[0].ParameterType == typeof(ILoggerFactory);
    }

    protected override IImportFilter? CreateInstance( Type type ) =>
        Activator.CreateInstance( type, _loggerFactory ) as IImportFilter;
}
