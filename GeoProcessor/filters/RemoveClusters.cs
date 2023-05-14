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

namespace J4JSoftware.GeoProcessor;

[BeforeImportFilters(DefaultFilterName, 10)]
public class RemoveClusters : ImportFilter
{
    public const string DefaultFilterName = "Remove Clusters";

    private Distance _maxClusterDiameter = new( UnitType.Meters, GeoConstants.DefaultMaxClusterDiameterMeters );

    public RemoveClusters(
        ILoggerFactory? loggerFactory
    )
    :base( loggerFactory)
    {
    }

    public Distance MaximumClusterDiameter
    {
        get => _maxClusterDiameter;

        set =>
            _maxClusterDiameter = value.Value <= 0
                ? new Distance( UnitType.Meters, GeoConstants.DefaultMaxClusterDiameterMeters )
                : value;
    }

    public override List<IImportedRoute> Filter( List<IImportedRoute> input )
    {
        if( !input.Any() )
        {
            Logger?.LogInformation( "Nothing to filter" );
            return input;
        }

        var retVal = new List<IImportedRoute>();

        foreach( var route in input )
        {
            retVal.Add( FilterRoute(route) );
        }

        return retVal;
    }

    private IImportedRoute FilterRoute( IImportedRoute toFilter )
    {
        var retVal = new ImportedRoute() { RouteName = toFilter.RouteName };

        Point? clusterOrigin = null;

        foreach( var coordinate in toFilter )
        {
            if( clusterOrigin == null )
            {
                clusterOrigin = coordinate;
                retVal.Points.Add( coordinate );

                continue;
            }

            var ptPair = new PointPair( clusterOrigin, coordinate );
            if( ptPair.GetDistance() <= MaximumClusterDiameter )
                continue;

            retVal.Points.Add( coordinate );
            clusterOrigin = coordinate;
        }

        return retVal;
    }
}
