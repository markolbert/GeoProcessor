#region license

// Copyright 2021 Mark A. Olbert
// 
// This library or program 'GeoProcessor' is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public License as
// published by the Free Software Foundation, either version 3 of the License,
// or (at your option) any later version.
// 
// This library or program is distributed in the hope that it will be useful, but
// WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// General Public License for more details.
// 
// You should have received a copy of the GNU General Public License along with
// this library or program.  If not, see <https://www.gnu.org/licenses/>.

#endregion

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BingMapsRESTToolkit;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.GeoProcessor;

[ RouteProcessor( ProcessorType.Bing ) ]
public class BingProcessor : CloudRouteProcessor
{
    public BingProcessor(
        IImportConfig config,
        ILoggerFactory? loggerFactory
    )
        : base( config, ProcessorType.Bing, loggerFactory )
    {
        Type = GeoExtensions.GetTargetType<RouteProcessorAttribute>( GetType() )!.Type;
    }

    public ProcessorType Type { get; }

    protected override async Task<List<Coordinate>?> ExecuteRequestAsync(
        List<Coordinate> coordinates,
        CancellationToken cancellationToken = default )
    {
        var request = new SnapToRoadRequest
        {
            BingMapsKey = ApiKey,
            IncludeSpeedLimit = false,
            IncludeTruckSpeedLimit = false,
            Interpolate = true,
            SpeedUnit = SpeedUnitType.MPH,
            TravelMode = TravelModeType.Driving,
            Points = coordinates.Select( p => p.ToBingMapsCoordinate() ).ToList()
        };

        var result = await request.Execute();

        if( result.StatusCode != 200 )
        {
            Logger?.LogError( "Snap to road request failed, message was '{description}'", result.StatusDescription );
            return null;
        }

        var retVal = new List<Coordinate>();

        foreach( var resourceSet in result.ResourceSets )
        {
            var snapResponses = resourceSet.Resources
                                           .Where( r => r is SnapToRoadResponse )
                                           .Cast<SnapToRoadResponse>()
                                           .ToList();

            if( !snapResponses.Any() )
            {
                Logger?.LogError( "Snap to request did not return usable results" );
                return null;
            }

            foreach( var snapResponse in snapResponses )
                retVal.AddRange( snapResponse.SnappedPoints
                                             .Select( p => new Coordinate( p ) )
                );
        }

        return retVal;
    }
}