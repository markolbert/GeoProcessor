using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.GeoProcessor;

[ImportFilter("Consolidate Points", 100)]
public class ConsolidatePoints : ImportFilter
{
    private double _minSep = GeoConstants.DefaultMinimumPointGapMeters;
    private double _maxOverallGap = GeoConstants.DefaultMaximumOverallGapMeters;

    public ConsolidatePoints(
        ILoggerFactory? loggerFactory
    )
    : base( loggerFactory )
    {
    }

    // meters
    public double MinimumPointGap
    {
        get => _minSep;
        set => _minSep = value < 0 ? GeoConstants.DefaultMinimumPointGapMeters : value;
    }

    // meters
    public double MaximumOverallGap
    {
        get => _maxOverallGap;
        set => _maxOverallGap = value < 0 ? GeoConstants.DefaultMaximumOverallGapMeters : value;
    }

    public override List<ImportedRoute> Filter( List<ImportedRoute> input )
    {
        var retVal = new List<ImportedRoute>();

        foreach( var rawRoute in input )
        {
            Coordinate2? prevPoint = null;
            Coordinate2? originPoint = null;

            var filteredRoute = new ImportedRoute()
            {
                RouteName = rawRoute.RouteName, 
                Description = rawRoute.Description
            };

            foreach( var curPoint in rawRoute.Points )
            {
                if( prevPoint == null || originPoint == null )
                {
                    filteredRoute.Points.Add( curPoint );
                    continue;
                }

                var curPair = new PointPair( prevPoint, curPoint );
                // need to convert to meters
                var curGap = curPair.GetDistance() * 1000;

                var originPair = new PointPair( originPoint, curPoint );
                // need to convert to meters
                var originGap = originPair.GetDistance() * 1000;
                
                prevPoint = curPoint;

                if( curGap >= MinimumPointGap )
                {
                    filteredRoute.Points.Add(curPoint);
                    originPoint = curPoint;

                    continue;
                }

                Logger?.LogTrace("Points within minimum gap: ({lat1}, {long1}), ({lat2}, {long2})",
                                  prevPoint.Latitude,
                                  prevPoint.Longitude,
                                  curPoint.Latitude,
                                  curPoint.Longitude);

                if (originGap >= MaximumOverallGap)
                {
                    filteredRoute.Points.Add(curPoint);
                    originPoint = curPoint;
                    continue;
                }

                Logger?.LogTrace("Points within maximum gap: ({lat1}, {long1}), ({lat2}, {long2})",
                                  originPoint.Latitude,
                                  originPoint.Longitude,
                                  curPoint.Latitude,
                                  curPoint.Longitude);
            }

            if( filteredRoute.Points.Count > 1 )
                retVal.Add( filteredRoute );
            else
                Logger?.LogInformation( "Route {name} {text}, excluding",
                                         filteredRoute.RouteName,
                                         filteredRoute.Points.Count switch
                                         {
                                             0 => "has no points",
                                             _ => "has only 1 point"
                                         } );
        }

        return retVal;
    }
}
