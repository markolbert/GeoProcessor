#region copyright
// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// BingProcessor.cs
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
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using BingMapsRESTToolkit;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.GeoProcessor;

[ RouteProcessor( "Bing" ) ]
public partial class BingProcessor : RouteProcessor
{
    [GeneratedRegex(@"[^\d]*(\d+)[^\d]*(\d+)[^\d]*(\d+\.*\d*)([^\.]*)")]
    private static partial Regex MaxGapRegEx();

    private List<SnappedImportedRoute>? _processedChunks;

    public BingProcessor(
        int maxPointsPerRequest = 100,
        ILoggerFactory? loggerFactory = null
    )
        : base( maxPointsPerRequest,
                null,
                loggerFactory,
                new InterpolatePoints( loggerFactory )
                {
                    MaximumPointSeparation = new Distance( UnitType.Kilometers, 2.5 )
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

        var maxGap = new Distance( UnitType.Kilometers, 2.5 );

        if( gap > maxGap )
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
            var request = new SnapToRoadRequest
            {
                BingMapsKey = ApiKey,
                IncludeSpeedLimit = false,
                IncludeTruckSpeedLimit = false,
                Interpolate = true,
                SpeedUnit = SpeedUnitType.MPH,
                TravelMode = TravelModeType.Driving,
                Points = routeChunk.Select(p => new BingMapsRESTToolkit.Coordinate(p.Latitude, p.Longitude))
                                   .ToList()
            };

            Response? result = null;

            try
            {
                result = await request.Execute().WaitAsync(RequestTimeout, ctx);
            }
            catch ( TimeoutException )
            {
                await HandleTimeoutExceptionAsync();
                continue;
            }
            catch( Exception ex )
            {
                var mesg = ex.Message.Contains("distance between point")
                    ? ParseGapException(routeChunk.ToList(), ex.Message)
                    : ex.Message;

                await HandleOtherRequestExceptionAsync( mesg );
                continue;
            }

            if( result == null )
            {
                await HandleOtherRequestExceptionAsync( "No response received from Bing Maps" );
                continue;
            }

            if (result.StatusCode != 200)
            {
                await HandleInvalidStatusCodeAsync(result.StatusDescription);

                foreach( var error in result.ErrorDetails )
                {
                    await HandleInvalidStatusCodeAsync( error );
                }

                continue;
            }

            foreach (var resourceSet in result.ResourceSets)
            {
                await ProcessResourceSetAsync(resourceSet, routeChunk);
            }
        }

        return _processedChunks;
    }

    private string ParseGapException(List<Coordinates> points, string mesg)
    {
        var matches = MaxGapRegEx().Matches(mesg);

        var match = matches.FirstOrDefault();
        if (match == null || match.Groups.Count < 5)
            return mesg;

        if (!int.TryParse(match.Groups[1].Value, out var pt1Idx) || pt1Idx >= points.Count - 1)
            return mesg;

        if (!int.TryParse(match.Groups[2].Value, out var pt2Idx) || pt2Idx >= points.Count - 1)
            return mesg;

        var gap = points[pt1Idx].GetDistance(points[pt2Idx]);

        return
            $"The gap ({gap.Value}) between point {pt1Idx:n0} ({points[pt1Idx].Latitude}, {points[pt1Idx].Longitude}) and {pt2Idx:n0} ({points[pt2Idx].Latitude}, {points[pt2Idx].Longitude}) exceeds {match.Groups[3].Value} {match.Groups[4].Value.Trim()}";
    }

    private async Task ProcessResourceSetAsync( ResourceSet resourceSet, ImportedRouteChunk routeChunk )
    {
        var snapResponses = resourceSet.Resources
                                       .Where( r => r is SnapToRoadResponse )
                                       .Cast<SnapToRoadResponse>()
                                       .ToList();

        if( !snapResponses.Any() )
            await SendMessage( ExpandedPhase,
                               "Snap to request did not return usable results",
                               false,
                               true,
                               LogLevel.Error );
        else
        {
            var snapResult = new SnappedImportedRoute( routeChunk,
                                                       snapResponses.SelectMany( x => x.SnappedPoints.Select(
                                                                         y => new Coordinates(
                                                                             y.Coordinate.Latitude,
                                                                             y.Coordinate.Longitude ) ) )
                                                                    .ToList() );

            _processedChunks!.Add( snapResult );
        }
    }
}
