#region copyright
// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// ConsolidateAlongBearing.cs
//
// This file is part of JumpForJoy Software's GeoProcessor.
// 
// GeoProcessor is free software: you can redistribute it and/or modify it 
// under the terms of the GNU General Public License as published by the 
// Free Software Foundation, either version 3 of the License, or 
// (at your option) any later version.
// 
// GeoProcessor is distributed in the hope that it will be useful, but 
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
// or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License 
// for more details.
// 
// You should have received a copy of the GNU General Public License along 
// with GeoProcessor. If not, see <https://www.gnu.org/licenses/>.
#endregion

using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.RouteSnapper;

[BeforeImportFilters(DefaultFilterName, 40)]
public class ConsolidateAlongBearing : ImportFilter
{
    public const string DefaultFilterName = "Consolidate Along Bearing";

    private double _bearingTolerance = GeoConstants.DefaultBearingToleranceDegrees;

    private Distance _maxDistance = new( UnitType.Meters, GeoConstants.DefaultMaximumBearingDistanceMeters);

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

    public Distance MaximumConsolidationDistance
    {
        get => _maxDistance;

        set
        {
            var distValue = Math.Abs( value.Value );
            _maxDistance = value with { Value = distValue };
        }
    }

    public override List<Route> Filter( List<Route> input )
    {
        var retVal = new List<Route>();

        foreach( var rawRoute in input )
        {
            var filteredRoute = new Route
            {
                RouteName = rawRoute.RouteName, Description = rawRoute.Description
            };

            Point? bearingOrigin = null;
            Point? distOrigin = null;
            double? prevBearing = null;

            foreach( var curPoint in rawRoute.Points )
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
