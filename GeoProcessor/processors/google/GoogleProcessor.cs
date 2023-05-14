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

namespace J4JSoftware.GeoProcessor;

[ RouteProcessor( "Google" ) ]
public class GoogleProcessor : RouteProcessor
{
    private const string RequestTemplate = "https://roads.googleapis.com/v1/snapToRoads?path={points}&interpolate={interpolate}&key={apiKey}";

    private List<SnappedImportedRoute>? _processedChunks;

    public GoogleProcessor(
        int maxPointsPerRequest = 100,
        ILoggerFactory? loggerFactory = null
    )
        : base( maxPointsPerRequest,
                null,
                loggerFactory,
                new InterpolatePoints( loggerFactory )
                {
                    MaximumPointSeparation = new Distance( UnitType.Meters, 300 )
                } )
    {
    }

    protected override List<IImportFilter> AdjustImportFilters()
    {
        var currentFilters = base.AdjustImportFilters();

        // ensure max gap values are consistent
        var interpolateFilter = currentFilters.FirstOrDefault( x => x.FilterName == InterpolatePoints.DefaultFilterName )
            as InterpolatePoints;

        var consolBearingFilter = currentFilters.FirstOrDefault( x => x.FilterName == ConsolidateAlongBearing.DefaultFilterName ) 
            as ConsolidateAlongBearing;

        if( interpolateFilter == null || consolBearingFilter == null )
            return currentFilters;

        var gap = interpolateFilter.MaximumPointSeparation;

        gap = consolBearingFilter.MaximumConsolidationDistance > gap
            ? gap
            : consolBearingFilter.MaximumConsolidationDistance;

        var maxGap = new Distance(UnitType.Meters, 300);

        if (gap > maxGap)
            gap = maxGap;

        interpolateFilter.MaximumPointSeparation = gap;
        consolBearingFilter.MaximumConsolidationDistance = gap;

        return currentFilters;
    }

    protected override async Task<List<SnappedImportedRoute>> ProcessRouteChunksAsync(
        List<ImportedRouteChunk> routeChunks,
        CancellationToken ctx
    )
    {
        _processedChunks = new List<SnappedImportedRoute>();

        foreach( var routeChunk in routeChunks )
        {
            var pointsText = routeChunk.Aggregate<Point, StringBuilder, string>( new StringBuilder(),
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
            GoogleResponse? result = null;

            try
            {
                result = await httpClient.GetFromJsonAsync<GoogleResponse>( url, ctx )
                                         .WaitAsync( RequestTimeout, ctx );
            }
            catch( TimeoutException )
            {
                await HandleTimeoutExceptionAsync();
                continue;
            }
            catch( Exception ex )
            {
                await HandleOtherRequestExceptionAsync( ex.Message );
                continue;
            }

            if( result == null )
            {
                await HandleInvalidStatusCodeAsync( "Snap to road request failed" );
                continue;
            }

            if( !string.IsNullOrEmpty( result.WarningMessage ) )
            {
                await HandleInvalidStatusCodeAsync( result.WarningMessage );
                continue;
            }

            var snappedRoute = new SnappedImportedRoute( routeChunk,
                                                         result.SnappedPoints
                                                               .Select( p => new Point( p.Location.Latitude,
                                                                            p.Location.Longitude ) )
                                                               .ToList() );

            _processedChunks.Add( snappedRoute );
        }

        return _processedChunks;
    }
}
