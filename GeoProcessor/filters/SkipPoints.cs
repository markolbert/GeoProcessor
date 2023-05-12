#region copyright
// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// SkipPoints.cs
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

namespace J4JSoftware.GeoProcessor;

[PostSnappingFilter(DefaultFilterName, 100)]
public class SkipPoints : ImportFilter
{
    public const string DefaultFilterName = "Skip Points";

    private Distance2 _maxGap = new( UnitType.Meters, GeoConstants.DefaultMaximumOverallGapMeters );

    public SkipPoints(
        ILoggerFactory? loggerFactory
    )
    : base( loggerFactory )
    {
    }

    public Distance2 MaximumGap
    {
        get => _maxGap;

        set =>
            _maxGap = value.Value < 0
                ? new Distance2( UnitType.Meters, GeoConstants.DefaultMaximumOverallGapMeters )
                : value;
    }

    public override List<IImportedRoute> Filter( List<IImportedRoute> input )
    {
        var retVal = new List<IImportedRoute>();

        foreach( var rawRoute in input )
        {
            Coordinate2? originPoint = null;

            var filteredRoute = new ImportedRoute()
            {
                RouteName = rawRoute.RouteName, 
                Description = rawRoute.Description
            };

            foreach( var curPoint in rawRoute )
            {
                if( originPoint == null )
                {
                    filteredRoute.Points.Add( curPoint );
                    originPoint = curPoint;
                    continue;
                }

                var originPair = new PointPair( originPoint, curPoint );
                var originGap = originPair.GetDistance();
                
                if( originGap < MaximumGap )
                    continue;

                filteredRoute.Points.Add(curPoint);
                originPoint = curPoint;
            }

            if( filteredRoute.Points.Count > 0 )
                retVal.Add( filteredRoute );
        }

        return retVal;
    }
}
