using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

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
    public uint Priority { get; }
    public string? Description { get; }
}

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
    public uint Priority { get; }
    public string? Description { get; }
}


[AttributeUsage(AttributeTargets.Class, Inherited = false)]
internal class AfterUserFiltersAttribute : Attribute
{
    public AfterUserFiltersAttribute(
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
    public string? Description { get; }
}
