using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.GeoProcessor;

[AfterAllImportFilter("Interpolate Points", 0)]
public class InterpolatePoints : ImportFilter
{
    private double _maxGap = GeoConstants.DefaultMaxPointSeparationKm;
    private ImportedRoute? _filteredRoute;

    public InterpolatePoints(
        ILoggerFactory? loggerFactory
    )
    : base( loggerFactory )
    {
    }

    // meters
    public double MaximumPointGap
    {
        get => _maxGap;
        set => _maxGap = value <= 0 ? GeoConstants.DefaultMaxPointSeparationKm : value;
    }

    public override List<ImportedRoute> Filter( List<ImportedRoute> input )
    {
        var retVal = new List<ImportedRoute>();

        foreach (var rawRoute in input)
        {
            Coordinate2? prevPoint = null;

            _filteredRoute = new ImportedRoute(rawRoute.RouteName, new List<Coordinate2>());

            foreach (var curPoint in rawRoute.Coordinates)
            {
                if (prevPoint == null )
                {
                    _filteredRoute.Coordinates.Add(curPoint);
                    continue;
                }

                var ptPair = new PointPair(prevPoint, curPoint);
                var gap = ptPair.GetDistance();

                prevPoint = curPoint;

                if (gap <= MaximumPointGap)
                {
                    _filteredRoute.Coordinates.Add(curPoint);
                    continue;
                }

                Logger?.LogTrace("Points exceed maximum gap ({gap}), interpolating: ({lat1}, {long1}), ({lat2}, {long2})",
                                  MaximumPointGap,
                                  prevPoint.Latitude,
                                  prevPoint.Longitude,
                                  curPoint.Latitude,
                                  curPoint.Longitude);

                Interpolate( ptPair, gap );
            }

            if( _filteredRoute.Coordinates.Count > 1 )
                retVal.Add( _filteredRoute );
            else
                Logger?.LogInformation( "Route {name} {text}, excluding",
                                         _filteredRoute.RouteName,
                                         _filteredRoute.Coordinates.Count switch
                                         {
                                             0 => "has no points",
                                             _ => "has only 1 point"
                                         } );
        }

        return retVal;
    }

    private void Interpolate( PointPair ptPair, double gap )
    {
        var steps = (int) Math.Ceiling( gap / MaximumPointGap );

        var deltaLat = ( ptPair.Second.Latitude - ptPair.First.Latitude ) / steps;
        var deltaLong = ( ptPair.Second.Longitude - ptPair.First.Longitude ) / steps;
        var deltaElevation = ( ptPair.Second.Elevation - ptPair.First.Elevation ) / steps;
        var deltaTime = ( ptPair.Second.Timestamp - ptPair.First.Timestamp ) / steps;

        for( var idx = 0; idx <= steps; idx++ )
        {
            var interpolated = new Coordinate2( ptPair.First.Latitude + idx * deltaLat,
                                                ptPair.First.Longitude + idx * deltaLong,
                                                true )
            {
                Elevation = ptPair.First.Elevation + idx * deltaElevation,
                Timestamp = ptPair.First.Timestamp + idx * deltaTime
            };

            _filteredRoute!.Coordinates.Add( interpolated );
        }
    }
}
