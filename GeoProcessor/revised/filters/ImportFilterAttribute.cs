using System;

namespace J4JSoftware.GeoProcessor;

[AttributeUsage(AttributeTargets.Class, Inherited=false)]
public class ImportFilterAttribute : Attribute
{
    public ImportFilterAttribute(
        string filterName
    )
    {
        FilterName = filterName;
    }

    public string FilterName { get; }
}
