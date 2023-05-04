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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GoogleApi;
using GoogleApi.Entities.Maps.Roads.Common;
using GoogleApi.Entities.Maps.Roads.SnapToRoads.Request;
using GoogleApi.Entities.Maps.Roads.SnapToRoads.Response;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.GeoProcessor;

[ RouteProcessor( ProcessorType.Google ) ]
public class GoogleProcessor : CloudRouteProcessor
{
    public GoogleProcessor(
        IImportConfig config,
        ILoggerFactory? loggerFactory
    )
        : base( config, ProcessorType.Google, loggerFactory )
    {
        Type = GeoExtensions.GetTargetType<RouteProcessorAttribute>( GetType() )!.Type;
    }

    public ProcessorType Type { get; }

    protected override async Task<List<Coordinate>?> ExecuteRequestAsync(
        List<Coordinate> coordinates,
        CancellationToken cancellationToken = default )
    {
        var request = new SnapToRoadsRequest
        {
            Interpolate = true,
            Key = ApiKey,
            Path = coordinates.Select( c => new GoogleApi.Entities.Maps.Roads.Common.Coordinate( c.Latitude, c.Longitude ) )
        };

        SnapToRoadsResponse? result;

        try
        {
            result = await GoogleMaps.Roads.SnapToRoad.QueryAsync( request, cancellationToken );
        }
        catch( Exception e )
        {
            Logger?.LogError( "Snap to road request failed. Message was '{mesg}'", e.Message );
            return null;
        }

        if( result == null )
        {
            Logger?.LogError( "Snap to road request failed" );
            return null;
        }

        var errors = result.Errors?.ToList() ?? new List<Error>();

        if( errors.Count <= 0 )
            return result.SnappedPoints
                         .Select( p => new Coordinate( p.Location.Latitude, p.Location.Longitude ) )
                         .ToList();

        foreach( var error in errors ) Logger?.LogError( "Snap to road error: {mesg}", error.ErrorMessage );

        return null;
    }
}