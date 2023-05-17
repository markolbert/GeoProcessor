# Snapping Routes

Once you've configured an instance of `RouteBuilder` you engage the snapping process by calling its `BuildAsync()` method:

```csharp
public async Task<BuildResults> BuildAsync( CancellationToken ctx = default )
```

Assuming no problems were encountered, calling `BuildAsync()` creates whatever output files you specified and returns an instance of `BuildResults`:

```csharp
public class BuildResults
{
    public virtual bool Succeeded { get; }

    public List<Route>? ImportedRoutes { get; set; }
    public List<Route>? FilteredRoutes { get; set; }
    public List<SnappedRoute>? SnappedRoutes { get; set; }
    public List<string>? Problems { get; set; }
}
```

|Property|Type|Comments|
|--------|----|--------|
|`Succeeded`|`bool`|True if the snapping process succeeded, false otherwise|
|`ImportedRoutes`|`List<Route>?`|the raw imported route data|
|`FilteredRoutes`|`List<Route>?`|the route data after it's been filtered|
|`SnappedRoutes`|`List<SnappedRoute>?`|the snapped route data|
|`Problems`|`List<string>?`|problems encountered during the snapping process|

[return to overview](overview.md#j4jsoftwaregeoprocessor-overview)
