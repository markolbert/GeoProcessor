using System.Collections.Generic;

namespace J4JSoftware.GeoProcessor;

public interface IFilteredEnumerable
{
    IEnumerable<Point> GetFilteredPoints(Distance? minSeparation = null, Distance? maxOverallGap = null);
    bool Any(Distance? minSeparation = null, Distance? maxOverallGap = null);
}