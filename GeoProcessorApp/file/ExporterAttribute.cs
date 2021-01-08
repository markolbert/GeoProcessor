using System;

namespace J4JSoftware.GeoProcessor
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class ExporterAttribute : Attribute
    {
        public ExporterAttribute(ExportType type)
        {
            Type = type;
        }

        public ExportType Type { get; }
    }
}