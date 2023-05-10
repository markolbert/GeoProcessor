using System;

namespace J4JSoftware.GeoProcessor;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
internal class BeforeUserFiltersAttribute : Attribute
{
    public BeforeUserFiltersAttribute(
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
    public ImportFilterCategory Category => ImportFilterCategory.BeforeUserDefined;
    public uint Priority { get; }
    public string? Description { get; }
}
