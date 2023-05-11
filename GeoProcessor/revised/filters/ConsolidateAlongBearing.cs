using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.GeoProcessor;

[BeforeImportFilters(DefaultFilterName, 40)]
public class ConsolidateAlongBearing : ImportFilter
{
    public const string DefaultFilterName = "Consolidate Along Bearing";

    private double _bearingTolerance = GeoConstants.DefaultBearingToleranceDegrees;

    private Distance2 _maxDistance = new( UnitType.Meters, GeoConstants.DefaultMaximumBearingDistanceMeters);

    public ConsolidateAlongBearing(
        ILoggerFactory? loggerFactory
    )
    : base( loggerFactory )
    {
    }

    public double BearingToleranceDegrees
    {
        get => _bearingTolerance;
        set => _bearingTolerance = value < 0 ? GeoConstants.DefaultBearingToleranceDegrees : value;
    }

    public Distance2 MaximumConsolidationDistance
    {
        get => _maxDistance;

        set
        {
            var distValue = Math.Abs( value.Value );
            _maxDistance = value with { Value = distValue };
        }
    }

    public override List<IImportedRoute> Filter( List<IImportedRoute> input )
    {
        var retVal = new List<IImportedRoute>();

        foreach( var rawRoute in input )
        {
            var filteredRoute = new ImportedRoute()
            {
                RouteName = rawRoute.RouteName, Description = rawRoute.Description
            };

            Coordinate2? bearingOrigin = null;
            Coordinate2? distOrigin = null;
            double? prevBearing = null;

            foreach( var curPoint in rawRoute )
            {
                if( distOrigin == null )
                {
                    filteredRoute.Points.Add( curPoint );
                    distOrigin = curPoint;
                    continue;
                }

                var distPair = new PointPair( distOrigin, curPoint );
                var curGap = distPair.GetDistance();

                if( curGap > MaximumConsolidationDistance  )
                {
                    distOrigin = curPoint;
                    filteredRoute.Points.Add( curPoint );
                    continue;
                }

                if( bearingOrigin == null )
                {
                    bearingOrigin = distOrigin;
                    distOrigin = curPoint;
                    prevBearing = null;

                    filteredRoute.Points.Add(curPoint);
                    continue;
                }

                var bearingPair = new PointPair(bearingOrigin, curPoint);
                var curBearing = bearingPair.GetBearing(true);

                if( !prevBearing.HasValue )
                {
                    prevBearing = curBearing;
                    filteredRoute.Points.Add(curPoint);

                    continue;
                }

                var bearingChange = Math.Abs( curBearing - prevBearing.Value );

                if( bearingChange <= BearingToleranceDegrees )
                    continue;

                Logger?.LogTrace(
                    "Bearing ({bearing}) or gap ({gap}) outside tolerances ({maxBearing}, {maxGap}): ({lat1}, {long1}), ({lat2}, {long2})",
                    curBearing,
                    $"{curGap.Value:n2} {curGap.Units}",
                    $"{BearingToleranceDegrees:n1}",
                    $"{MaximumConsolidationDistance.Value:n2} {MaximumConsolidationDistance.Units}",
                    $"{bearingOrigin.Latitude:n4}",
                    $"{bearingOrigin.Longitude:n4}",
                    $"{curPoint.Latitude:n4}",
                    $"{curPoint.Longitude:n4}" );

                filteredRoute.Points.Add(curPoint);
                prevBearing = curBearing;
                bearingOrigin = curPoint;
                distOrigin = curPoint;
            }

            retVal.Add( filteredRoute );
        }

        return retVal;
    }
}
