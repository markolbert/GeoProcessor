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

namespace J4JSoftware.RouteSnapper;

[ RouteProcessor( "Bing" ) ]
public partial class BingProcessor : RouteProcessor
{
    [GeneratedRegex(@"[^\d]*(\d+)[^\d]*(\d+)[^\d]*(\d+\.*\d*)([^\.]*)")]
    private static partial Regex MaxGapRegEx();

    public BingProcessor(
        int maxPointsPerRequest = 100,
        ILoggerFactory? loggerFactory = null
    )
        : base(maxPointsPerRequest, null, loggerFactory)
    {
        MaximumOverallPointGap = new Distance(UnitType.Kilometers, 2.5);
    }

    protected override async Task<List<Point>?> ProcessRouteChunkAsync( List<Point> srcPoints, CancellationToken ctx )
    {
        var request = new SnapToRoadRequest
        {
            BingMapsKey = ApiKey,
            IncludeSpeedLimit = false,
            IncludeTruckSpeedLimit = false,
            Interpolate = true,
            SpeedUnit = SpeedUnitType.MPH,
            TravelMode = TravelModeType.Driving,
            Points = srcPoints.Select( p => new Coordinate( p.Latitude, p.Longitude ) )
                                   .ToList()
        };

        Response? result;

        try
        {
            result = await request.Execute().WaitAsync( RequestTimeout, ctx );
        }
        catch( TimeoutException )
        {
            await HandleTimeoutExceptionAsync();
            return null;
        }
        catch( Exception ex )
        {
            var mesg = ex.Message.Contains( "distance between point" )
                ? ParseGapException( srcPoints, ex.Message )
                : ex.Message;

            await HandleOtherRequestExceptionAsync( mesg );
            return null;
        }

        if( result == null )
        {
            await HandleOtherRequestExceptionAsync( "No response received from Bing Maps" );
            return null;
        }

        if ( result.StatusCode != 200 )
        {
            await HandleInvalidStatusCodeAsync( result.StatusDescription );

            foreach( var error in result.ErrorDetails )
            {
                await HandleInvalidStatusCodeAsync( error );
            }

            return null;
        }

        var retVal = new List<Point>();

        foreach ( var resourceSet in result.ResourceSets )
        {
            var points = await ProcessResourceSetAsync( resourceSet );

            if( points != null && points.Any() )
                retVal.AddRange( points );
        }

        return retVal;
    }

    private string ParseGapException(List<Point> points, string mesg)
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

    private async Task<List<Point>?> ProcessResourceSetAsync( ResourceSet resourceSet )
    {
        var snapResponses = resourceSet.Resources
                                       .Where( r => r is SnapToRoadResponse )
                                       .Cast<SnapToRoadResponse>()
                                       .ToList();

        if( snapResponses.Any() )
            return snapResponses.SelectMany( x => x.SnappedPoints.Select(
                                                 y => new Point
                                                 {
                                                     Latitude = y.Coordinate.Latitude,
                                                     Longitude = y.Coordinate.Longitude
                                                 } ) )
                                .ToList();

        await SendMessage( ExpandedPhase,
                           "Snap to request did not return usable results",
                           false,
                           true,
                           LogLevel.Error );

        return null;
    }
}
