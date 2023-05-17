#region copyright
// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// ConsolidatePoints.cs
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

using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.RouteSnapper;

[BeforeImportFilters(DefaultFilterName, 50)]
public class ConsolidatePoints : ImportFilter
{
    public const string DefaultFilterName = "Consolidate Points";

    private Distance _minSep = new( UnitType.Meters, GeoConstants.DefaultMinimumPointGapMeters );
    private Distance _maxOverallGap = new( UnitType.Meters, GeoConstants.DefaultMaximumOverallGapMeters );

    public ConsolidatePoints(
        ILoggerFactory? loggerFactory
    )
    : base( loggerFactory )
    {
    }

    public Distance MinimumPointGap
    {
        get => _minSep;

        set =>
            _minSep = value.Value < 0
                ? new Distance( UnitType.Meters, GeoConstants.DefaultMinimumPointGapMeters )
                : value;
    }

    public Distance MaximumOverallGap
    {
        get => _maxOverallGap;

        set =>
            _maxOverallGap = value.Value < 0
                ? new Distance( UnitType.Meters, GeoConstants.DefaultMaximumOverallGapMeters )
                : value;
    }

    public override List<Route> Filter( List<Route> input )
    {
        var retVal = new List<Route>();

        foreach( var rawRoute in input )
        {
            Point? prevPoint = null;
            Point? originPoint = null;

            var filteredRoute = new Route
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
                var curGap = curPair.GetDistance();

                var originPair = new PointPair( originPoint, curPoint );
                var originGap = originPair.GetDistance();
                
                prevPoint = curPoint;

                if( curGap > MinimumPointGap )
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
