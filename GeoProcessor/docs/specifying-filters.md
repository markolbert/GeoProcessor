# Specifying Filters

There are a number of filters available for clarifying the data used to generate snapped routes. The filters are applied before route snapping occurs.

|Extension Method|Arguments|Comments|
|----------------|---------|--------|
|`ConsolidatePoints`||merges nearby points in a route|
||`Distance?` minPointGap = null|points closer than this value are merged, subject to the maximum overall gap constraint. Defaults to 200 meters|
||`Distance?` maxOverallGap = null|prevents points from being merged if the overall gap between the last unmerged point and the current point exceeds the value. Defaults to 1,000 meters|
|`ConsolidateAlongBearing`||merges points that lie along a constant bearing|
||`double` bearingToleranceDegrees|point to point bearings that lie within this value, in absolute value terms, are considered to be along the same bearing. Specify values in degrees. Defaults to 15 degrees.|
||`Distance?` maxConsolDist = null|prevents points from being merged if the  gap between the last unmerged point and the current point exceeds the value. Defaults to 2,500 meters|
|`MergeRoutes`||merges *routes* that are adjacent to each other. Adjacency is evaluated at both ends of each route|
||`Distance?` maxRouteGap = null|defines the maximum distance between route endpoints which will result in the routes being merged. Defaults to 500 meters|
|`RemoveClusters`||removes clusters of points that are near each other (e.g., perhaps because you were walking around at a stopover|
||`Distance?` maxClusterRadius = null|defines the maximum distance between points which will result in them being merged. Defaults to 500 meters|
|`RemoveGarminMessagePoints`||ignores the points related to SMS messages produced by Garmin InReach devices (and perhaps others).|

Distances are specified using instances of the `Distance` class, which is aware of both magnitude and measurement units:

```csharp
public record Distance( UnitType Units, double Value )

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
```

Conversion between various units is handled automatically by the library.

[return to configuration](overview.md#configuration-via-extension-methods)

[return to overview](overview.md#j4jsoftwaregeoprocessor-overview)
