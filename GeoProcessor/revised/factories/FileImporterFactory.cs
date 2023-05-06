using System;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.GeoProcessor;

public class FileImporterFactory : NamedTypeFactory<IFileImporter, ImportFileTypeAttribute>
{
    private readonly ILoggerFactory? _loggerFactory;

    public FileImporterFactory(
        ILoggerFactory? loggerFactory = null
    )
        : base( x => x.FileType, loggerFactory )
    {
        _loggerFactory = loggerFactory;
    }

    protected override bool CheckConstructor( ConstructorInfo ctor )
    {
        var ctorArgs = ctor.GetParameters();

        return ctorArgs.Length == 1 && ctorArgs[ 0 ].ParameterType == typeof( ILoggerFactory );
    }

    protected override IFileImporter? CreateInstance( Type type ) =>
        Activator.CreateInstance( type, _loggerFactory ) as IFileImporter;
}
