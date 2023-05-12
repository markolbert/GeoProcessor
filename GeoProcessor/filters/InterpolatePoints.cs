#region copyright
// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// InterpolatePoints.cs
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

namespace J4JSoftware.GeoProcessor;

[AfterImportFilters(DefaultFilterName, 0)]
public class InterpolatePoints : ImportFilter
{
    public const string DefaultFilterName = "Interpolate Points";

    private Distance2 _maxSeparation = new( UnitType.Kilometers, GeoConstants.DefaultMaxPointSeparationKm );
    private ImportedRoute? _filteredRoute;

    public InterpolatePoints(
        ILoggerFactory? loggerFactory
    )
    : base( loggerFactory )
    {
    }

    public Distance2 MaximumPointSeparation
    {
        get => _maxSeparation;

        set =>
            _maxSeparation = value.Value <= 0
                ? new Distance2( UnitType.Kilometers, GeoConstants.DefaultMaxPointSeparationKm )
                : value;
    }

    public override List<IImportedRoute> Filter( List<IImportedRoute> input )
    {
        var retVal = new List<IImportedRoute>();

        foreach (var rawRoute in input)
        {
            Coordinate2? prevPoint = null;

            _filteredRoute = new ImportedRoute() { RouteName = rawRoute.RouteName, Description = rawRoute.Description };

            foreach (var curPoint in rawRoute )
            {
                if (prevPoint == null )
                {
                    _filteredRoute.Points.Add(curPoint);
                    prevPoint = curPoint;

                    continue;
                }

                var ptPair = new PointPair(prevPoint, curPoint);
                var gap = ptPair.GetDistance();

                prevPoint = curPoint;

                if (gap <= MaximumPointSeparation)
                {
                    _filteredRoute.Points.Add(curPoint);
                    continue;
                }

                Logger?.LogTrace("Points exceed maximum gap ({gap}), interpolating: ({lat1}, {long1}), ({lat2}, {long2})",
                                  MaximumPointSeparation,
                                  prevPoint.Latitude,
                                  prevPoint.Longitude,
                                  curPoint.Latitude,
                                  curPoint.Longitude);

                Interpolate( ptPair, gap );
            }

            if( _filteredRoute.Points.Count > 1 )
                retVal.Add( _filteredRoute );
            else
                Logger?.LogInformation( "Route {name} {text}, excluding",
                                         _filteredRoute.RouteName,
                                         _filteredRoute.Points.Count switch
                                         {
                                             0 => "has no points",
                                             _ => "has only 1 point"
                                         } );
        }

        return retVal;
    }

    private void Interpolate( PointPair ptPair, Distance2 gap )
    {
        var steps = (int) Math.Ceiling( ( gap / MaximumPointSeparation ).Value );

        var deltaLat = ( ptPair.Second.Latitude - ptPair.First.Latitude ) / steps;
        var deltaLong = ( ptPair.Second.Longitude - ptPair.First.Longitude ) / steps;
        var deltaElevation = ( ptPair.Second.Elevation - ptPair.First.Elevation ) / steps;
        var deltaTime = ( ptPair.Second.Timestamp - ptPair.First.Timestamp ) / steps;

        for( var idx = 0; idx <= steps; idx++ )
        {
            var interpolationState = idx == 0
                ? InterpolationState.Start
                : idx == steps
                    ? InterpolationState.End
                    : InterpolationState.Intermediate;

            var interpolated = new Coordinate2( ptPair.First.Latitude + idx * deltaLat,
                                                ptPair.First.Longitude + idx * deltaLong,
                                                interpolationState )
            {
                Elevation = ptPair.First.Elevation + idx * deltaElevation,
                Timestamp = ptPair.First.Timestamp + idx * deltaTime
            };

            _filteredRoute!.Points.Add( interpolated );
        }
    }
}
