using System;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.GeoProcessor;

public class FileImporterFactory : NamedTypeFactory<IFileImporter>
{
    private readonly ILoggerFactory? _loggerFactory;

    public FileImporterFactory(
        ILoggerFactory? loggerFactory = null
    )
        : base( loggerFactory )
    {
        _loggerFactory = loggerFactory;
    }

    protected override bool CheckConstructor( ConstructorInfo ctor )
    {
        var ctorArgs = ctor.GetParameters();

        return ctorArgs.Length == 1 && ctorArgs[ 0 ].ParameterType == typeof( ILoggerFactory );
    }

    protected override NamedTypeAttributeInfo? GetAttributeInfo( Type itemType )
    {
        var attr = itemType.GetCustomAttribute<ImportFileTypeAttribute>();
        return attr == null ? null : new NamedTypeAttributeInfo( attr.FileType, attr );
    }

    protected override IFileImporter? CreateInstance( Type type ) =>
        Activator.CreateInstance( type, _loggerFactory ) as IFileImporter;
}
