using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.GeoProcessor;

public class FileImporterFactory
{
    private readonly List<Assembly> _assemblies = new();
    private readonly Dictionary<string, Type> _importerTypes = new( StringComparer.OrdinalIgnoreCase );
    private readonly ILoggerFactory? _loggerFactory;
    private readonly ILogger? _logger;

    public FileImporterFactory(
        ILoggerFactory? loggerFactory = null
    )
    {
        _loggerFactory = loggerFactory;
        _logger = loggerFactory?.CreateLogger<FileImporterFactory>();
    }

    public void ScanAssemblies( params Assembly[] assemblies ) => _assemblies.AddRange( assemblies );
    public void ScanAssemblies( params Type[] types ) => _assemblies.AddRange( types.Select( t => t.Assembly ) );

    public bool InitializeFactory( bool scanDefault = true )
    {
        if( scanDefault )
            _assemblies.Add( GetType().Assembly );

        if( !_assemblies.Any() )
            return false;

        _importerTypes.Clear();

        foreach( var importerType in _assemblies.Distinct()
                                                .SelectMany( a => a.GetTypes() )
                                                .Where( t => t.IsAssignableTo( typeof( IFileImporter ) )
                                                         && !t.IsAbstract
                                                         && t.GetConstructors()
                                                             .Any( c =>
                                                              {
                                                                  var ctorArgs = c.GetParameters();
                                                                  return ctorArgs.Length == 1
                                                                   && ctorArgs[ 0 ].ParameterType
                                                                   == typeof( ILoggerFactory );
                                                              } ) ) )
        {
            var attr = importerType.GetCustomAttribute<FileTypeAttribute>();
            if( attr == null )
                continue;

            if( !_importerTypes.TryAdd( attr.FileType, importerType ) )
                _logger?.LogError( "Duplicate importer for '{fileType}' files, ignoring", attr.FileType );
        }

        return true;
    }

    public IFileImporter? this[ string fileType ]
    {
        get
        {
            if( _importerTypes.TryGetValue( fileType, out var type ) )
                return Activator.CreateInstance( type, _loggerFactory ) as IFileImporter;

            _logger?.LogWarning( "Unsupported file importer type '{fileType}'", fileType );
            return null;
        }
    }
}
