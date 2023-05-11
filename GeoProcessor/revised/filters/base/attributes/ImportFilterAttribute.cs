using System;

namespace J4JSoftware.GeoProcessor;

[AttributeUsage(AttributeTargets.Class, Inherited=false)]
public class ImportFilterAttribute : Attribute
{
    public ImportFilterAttribute(
        string filterName,
        uint priority,
        string? description = null
    )
    {
        FilterName = filterName;
        Priority = priority;
        Description = description;
    }

    public string FilterName { get; }
    public ImportFilterCategory Category => ImportFilterCategory.UserDefined;
    public uint Priority { get; }
    public string? Description { get; }
}