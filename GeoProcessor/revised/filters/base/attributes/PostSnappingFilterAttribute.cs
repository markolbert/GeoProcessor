using System;

namespace J4JSoftware.GeoProcessor;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class PostSnappingFilterAttribute : Attribute
{
    public PostSnappingFilterAttribute(
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
    public uint Priority { get; }
    public ImportFilterCategory Category => ImportFilterCategory.PostSnapping;
    public string? Description { get; }
}
