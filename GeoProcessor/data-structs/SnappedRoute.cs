﻿using System.Collections.Generic;

namespace J4JSoftware.GeoProcessor;

public class SnappedRoute
{
    public string? RouteName { get; set; }
    public string? Description { get; set; }

    public List<Point> SnappedPoints { get; } = new();
}
