#region copyright
// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// GoogleProcessor.cs
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
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GoogleRoads = GoogleApi.Entities.Maps.Roads;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.RouteSnapper;

[ RouteProcessor( "Google" ) ]
public class GoogleProcessor : RouteProcessor
{
    private const string RequestTemplate = "https://roads.googleapis.com/v1/snapToRoads?path={points}&interpolate={interpolate}&key={apiKey}";

    public GoogleProcessor(
        int maxPointsPerRequest = 100,
        ILoggerFactory? loggerFactory = null
    )
        : base( maxPointsPerRequest, null, loggerFactory )
    {
        MaximumOverallPointGap = new Distance(UnitType.Meters, 300);
    }

    protected override async Task<List<Point>?> ProcessRouteChunkAsync(
        List<Point> srcPoints,
        CancellationToken ctx
    )
    {
        var pointsText = srcPoints.Aggregate<Point, StringBuilder, string>( new StringBuilder(),
            ( sb, pt ) =>
            {
                if( sb.Length > 0 )
                    sb.Append( '|' );

                sb.Append( $"{Math.Round( pt.Latitude, 6 )},{Math.Round( pt.Longitude, 6 )}" );

                return sb;
            },
            sb => sb.ToString() );

        var url = RequestTemplate.Replace( "{points}", pointsText )
                                 .Replace( "{interpolate}", "true" )
                                 .Replace( "{apiKey}", ApiKey );

        var httpClient = new HttpClient();
        GoogleResponse? result;

        try
        {
            result = await httpClient.GetFromJsonAsync<GoogleResponse>( url, ctx )
                                     .WaitAsync( RequestTimeout, ctx );
        }
        catch( TimeoutException )
        {
            await HandleTimeoutExceptionAsync();
            return null;
        }
        catch( Exception ex )
        {
            await HandleOtherRequestExceptionAsync( ex.Message );
            return null;
        }

        if( result == null )
        {
            await HandleInvalidStatusCodeAsync( "Snap to road request failed" );
            return null;
        }

        if( string.IsNullOrEmpty( result.WarningMessage ) )
            return result.SnappedPoints
                         .Select( p => new Point { Latitude = p.Location.Latitude, Longitude = p.Location.Longitude } )
                         .ToList();

        await HandleInvalidStatusCodeAsync( result.WarningMessage );
        return null;
    }
}
