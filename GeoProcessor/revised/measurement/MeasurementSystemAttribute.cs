using System;

namespace J4JSoftware.GeoProcessor;

[AttributeUsage(AttributeTargets.Field)]
public class MeasurementSystemAttribute : Attribute
{
    public MeasurementSystemAttribute(
        MeasurementSystem system,
        uint scaleFactor
    )
    {
        MeasurementSystem = system;
        ScaleFactor = scaleFactor == 0 ? 1 : scaleFactor;
    }

    public MeasurementSystem MeasurementSystem { get; }
    public uint ScaleFactor { get; }
}
