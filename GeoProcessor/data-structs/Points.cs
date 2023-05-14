using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace J4JSoftware.GeoProcessor;

public class Points : IFilteredEnumerable
{
    private readonly List<Point> _points = new();

    public Points()
    {
    }

    public Points(Points src)
    {
        foreach (var point in src._points)
        {
            Add(point.Copy());
        }
    }

    public Points(IEnumerable<Point> src)
    {
        foreach (var point in src)
        {
            Add(point.Copy());
        }
    }

    public int Count => _points.Count;

    public Point? this[int idx]
    {
        get
        {
            if (idx < 0 || idx >= _points.Count)
                return null;

            return _points[idx];
        }
    }

    public void Clear() => _points.Clear();

    public void Add(Point point)
    {
        point.AddToCollection(this, _points.Count);
        _points.Add(point);
    }

    public void AddRange(IEnumerable<Point> toAdd)
    {
        foreach (var point in toAdd)
        {
            Add(point);
        }
    }

    public void ResetPassesFilter() => _points.ForEach(x => x.PassesFilter = false);

    public bool Any(Distance? minSeparation = null, Distance? maxOverallGap = null) =>
        GetFilteredPoints(minSeparation, maxOverallGap).Any();

    public IEnumerable<Point> GetFilteredPoints(Distance? minSeparation = null, Distance? maxOverallGap = null)
    {
        minSeparation ??= new Distance(UnitType.Meters, GeoConstants.DefaultMinimumPointGapMeters);
        maxOverallGap ??= new Distance(UnitType.Meters, GeoConstants.DefaultMaximumOverallGapMeters);

        var emittedFirstPoint = false;

        Point? originPoint = null;
        Point? prevPoint = null;

        foreach (var point in _points.Where(x => x.PassesFilter)
                     .OrderBy(x => x.Index))
        {
            if (!emittedFirstPoint)
            {
                emittedFirstPoint = true;
                yield return point;
                prevPoint = point;
                originPoint = point;
            }
            else
            {
                var curPair = new PointPair(prevPoint!, point);
                var curGap = curPair.GetDistance();

                if (curGap > minSeparation)
                {
                    if(curGap < maxOverallGap)
                        yield return point;
                    else
                    {
                        // interpolate
                        foreach (var interpolated in Interpolate(curPair, curGap, maxOverallGap))
                        {
                            yield return interpolated;
                        }
                    }
                }
                else
                {
                    var originGap = new PointPair(originPoint!, point).GetDistance();
                
                    if (originGap >= maxOverallGap)
                    {
                        yield return point;
                        originPoint = point;
                    }
                }

                prevPoint = point;
            }
        }
    }

    private IEnumerable<Point> Interpolate(PointPair ptPair, Distance gap, Distance maxGap)
    {
        var steps = (int)Math.Ceiling((gap / maxGap).Value);

        var deltaLat = (ptPair.Second.Latitude - ptPair.First.Latitude) / steps;
        var deltaLong = (ptPair.Second.Longitude - ptPair.First.Longitude) / steps;
        var deltaElevation = (ptPair.Second.Elevation - ptPair.First.Elevation) / steps;
        var deltaTime = (ptPair.Second.Timestamp - ptPair.First.Timestamp) / steps;

        for (var idx = 0; idx <= steps; idx++)
        {
            var interpolationState = idx == 0
                ? InterpolationState.Start
                : idx == steps
                    ? InterpolationState.End
                    : InterpolationState.Intermediate;

            var interpolated = new Point(ptPair.First.Latitude + idx * deltaLat,
                ptPair.First.Longitude + idx * deltaLong,
                interpolationState)
            {
                Elevation = ptPair.First.Elevation + idx * deltaElevation,
                Timestamp = ptPair.First.Timestamp + idx * deltaTime
            };

            yield return interpolated;
        }
    }
}