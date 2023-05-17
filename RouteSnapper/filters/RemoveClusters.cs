#region copyright
// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// RemoveClusters.cs
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
using System.Linq;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.RouteSnapper;

[BeforeImportFilters(DefaultFilterName, 10)]
public class RemoveClusters : ImportFilter
{
    public const string DefaultFilterName = "Remove Clusters";

    private Distance _maxClusterRadius = new( UnitType.Meters, GeoConstants.DefaultMaxClusterDiameterMeters );

    public RemoveClusters(
        ILoggerFactory? loggerFactory
    )
    :base( loggerFactory)
    {
    }

    public Distance MaximumClusterRadius
    {
        get => _maxClusterRadius;

        set =>
            _maxClusterRadius = value.Value <= 0
                ? new Distance( UnitType.Meters, GeoConstants.DefaultMaxClusterDiameterMeters )
                : value;
    }

    public override List<Route> Filter( List<Route> input )
    {
        if( !input.Any() )
        {
            Logger?.LogInformation( "Nothing to filter" );
            return input;
        }

        var retVal = new List<Route>();

        foreach( var route in input )
        {
            retVal.Add( FilterRoute(route) );
        }

        return retVal;
    }

    private Route FilterRoute( Route toFilter )
    {
        var retVal = new Route() { RouteName = toFilter.RouteName };

        Point? clusterOrigin = null;

        foreach( var point in toFilter.Points )
        {
            if( clusterOrigin == null )
            {
                clusterOrigin = point;
                retVal.Points.Add( point );

                continue;
            }

            var ptPair = new PointPair( clusterOrigin, point );
            if( ptPair.GetDistance() <= MaximumClusterRadius )
                continue;

            retVal.Points.Add( point );
            clusterOrigin = point;
        }

        return retVal;
    }
}
