using System;

namespace J4JSoftware.GeoProcessor
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class RouteProcessorAttribute : Attribute
    {
        public RouteProcessorAttribute(ProcessorType type)
        {
            Type = type;
        }

        public ProcessorType Type { get; }
    }
}