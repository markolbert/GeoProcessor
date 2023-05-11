using System;

namespace J4JSoftware.GeoProcessor;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
internal class AfterImportFiltersAttribute : Attribute
{
    public AfterImportFiltersAttribute(
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
    public ImportFilterCategory Category => ImportFilterCategory.AfterUserDefined;
    public string? Description { get; }
}
