using System.Collections.Generic;

namespace J4JSoftware.RouteSnapper;

public class RouteProcessorResult
{
    public virtual bool Succeeded => FilteredRoutes != null && SnappedRoutes != null && Problems == null;

    public List<Route>? FilteredRoutes { get; set; }
    public List<SnappedRoute>? SnappedRoutes { get; set; }
    public List<string>? Problems { get; set; }
}
