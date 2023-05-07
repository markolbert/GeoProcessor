using System;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.GeoProcessor;

public class RouteProcessorFactory : NamedTypeFactory<IRouteProcessor2>
{
    private readonly ILoggerFactory? _loggerFactory;

    public RouteProcessorFactory(
        ILoggerFactory? loggerFactory = null
    )
        : base( loggerFactory )
    {
        _loggerFactory = loggerFactory;
    }

    protected override bool CheckConstructor( ConstructorInfo ctor )
    {
        var ctorArgs = ctor.GetParameters();

        return ctorArgs.Length == 1 && ctorArgs[0].ParameterType == typeof(ILoggerFactory);
    }

    protected override NamedTypeAttributeInfo? GetAttributeInfo(Type itemType)
    {
        var attr = itemType.GetCustomAttribute<RouteProcessorAttribute2>();
        return attr == null ? null : new NamedTypeAttributeInfo(attr.Processor, attr);
    }

    protected override IRouteProcessor2? CreateInstance( Type type ) =>
        Activator.CreateInstance( type, _loggerFactory ) as IRouteProcessor2;
}
