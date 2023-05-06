using System;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.GeoProcessor;

public class RouteProcessorFactory : NamedTypeFactory<IRouteProcessor2, RouteProcessorAttribute2>
{
    private readonly ILoggerFactory? _loggerFactory;

    public RouteProcessorFactory(
        ILoggerFactory? loggerFactory = null
    )
        : base( x => x.Processor, loggerFactory )
    {
        _loggerFactory = loggerFactory;
    }

    protected override bool CheckConstructor( ConstructorInfo ctor )
    {
        var ctorArgs = ctor.GetParameters();

        return ctorArgs.Length == 1 && ctorArgs[0].ParameterType == typeof(ILoggerFactory);
    }

    protected override IRouteProcessor2? CreateInstance( Type type ) =>
        Activator.CreateInstance( type, _loggerFactory ) as IRouteProcessor2;
}
