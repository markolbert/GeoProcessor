namespace J4JSoftware.GeoProcessor;

public enum UnitType
{
    [MeasurementSystem(MeasurementSystem.American, 1)]
    Feet,

    [MeasurementSystem(MeasurementSystem.Metric, 1)]
    Meters,

    [MeasurementSystem(MeasurementSystem.American, 5280)]
    Miles,

    [MeasurementSystem(MeasurementSystem.Metric, 1000)]
    Kilometers
}
