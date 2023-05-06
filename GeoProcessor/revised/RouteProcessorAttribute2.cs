using System;

namespace J4JSoftware.GeoProcessor;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class RouteProcessorAttribute2 : Attribute
{
    public RouteProcessorAttribute2(
        string processor
    )
    {
        Processor = processor;
    }

    public string Processor { get; }
}
