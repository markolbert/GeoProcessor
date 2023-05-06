using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.GeoProcessor;

public class RouteProcessorFactory
{
    private readonly List<Assembly> _assemblies = new();
    private readonly Dictionary<string, Type> _procTypes = new( StringComparer.OrdinalIgnoreCase );
    private readonly ILoggerFactory? _loggerFactory;
    private readonly ILogger? _logger;

    public RouteProcessorFactory(
        ILoggerFactory? loggerFactory = null
    )
    {
        _loggerFactory = loggerFactory;
        _logger = loggerFactory?.CreateLogger<RouteProcessorFactory>();
    }

    public void ScanAssemblies( params Assembly[] assemblies ) => _assemblies.AddRange( assemblies );
    public void ScanAssemblies( params Type[] types ) => _assemblies.AddRange( types.Select( t => t.Assembly ) );

    public bool InitializeFactory( bool scanDefault = true )
    {
        if( scanDefault )
            _assemblies.Add( GetType().Assembly );

        if( !_assemblies.Any() )
            return false;

        _procTypes.Clear();

        foreach( var procType in _assemblies.Distinct()
                                                .SelectMany( a => a.GetTypes() )
                                                .Where( t => t.IsAssignableTo( typeof( IRouteProcessor2 ) )
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
            var attr = procType.GetCustomAttribute<RouteProcessorAttribute2>();
            if( attr == null )
                continue;

            if( !_procTypes.TryAdd( attr.Processor, procType ) )
                _logger?.LogError( "Duplicate route processor '{fileType}', ignoring", attr.Processor );
        }

        return true;
    }

    public IRouteProcessor2? this[ string processor ]
    {
        get
        {
            if( _procTypes.TryGetValue( processor, out var type ) )
                return Activator.CreateInstance( type, _loggerFactory ) as
                    IRouteProcessor2;

            _logger?.LogWarning( "Unsupported route processor '{processor}'", processor );
            return null;
        }
    }
}
