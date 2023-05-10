using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using BingMapsRESTToolkit;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.GeoProcessor;

[ RouteProcessorAttribute2( "Bing" ) ]
public partial class BingProcessor2 : RouteProcessor2
{
    [ GeneratedRegex( @"[^\d]*(\d+)[^\d]*(\d+)[^\d]*(\d+\.*\d*)([^\.]*)" ) ]
    private static partial Regex MaxGapRegEx();

    private List<SnappedImportedRoute>? _processedChunks;

    public BingProcessor2(
        int maxPointsPerRequest = 100,
        ILoggerFactory? loggerFactory = null
    )
        : base( maxPointsPerRequest,
                null,
                loggerFactory,
                new InterpolatePoints( loggerFactory )
                {
                    MaximumPointSeparation = new Distance2( UnitType.Kilometers, 2.5 )
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

        if( gap.Value > 2.5 )
            gap = gap with { Value = 2.5 };

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
                Points = routeChunk.Select( p => new BingMapsRESTToolkit.Coordinate( p.Latitude, p.Longitude ) )
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
                continue;
            }
            catch( Exception ex )
            {
                var mesg = ex.Message.Contains( "distance between point" )
                    ? ParseGapException( routeChunk.ToList(), ex.Message )
                    : ex.Message;

                await HandleOtherRequestExceptionAsync( mesg );
                continue;
            }

            if( result.StatusCode != 200 )
            {
                await HandleInvalidStatusCodeAsync( result.StatusDescription );
                continue;
            }

            foreach( var resourceSet in result.ResourceSets )
            {
                await ProcessResourceSetAsync( resourceSet, routeChunk );
            }
        }

        return _processedChunks;
    }

    private string ParseGapException( List<Coordinate2> points, string mesg )
    {
        var matches = MaxGapRegEx().Matches( mesg );

        var match = matches.FirstOrDefault();
        if( match == null || match.Groups.Count < 5 )
            return mesg;

        if( !int.TryParse( match.Groups[ 1 ].Value, out var pt1Idx ) || pt1Idx >= points.Count - 1 )
            return mesg;

        if( !int.TryParse( match.Groups[ 2 ].Value, out var pt2Idx ) || pt2Idx >= points.Count - 1 )
            return mesg;

        var gap = points[ pt1Idx ].GetDistance( points[ pt2Idx ] );

        return
            $"The gap ({gap.Value}) between point {pt1Idx:n0} ({points[ pt1Idx ].Latitude}, {points[ pt1Idx ].Longitude}) and {pt2Idx:n0} ({points[ pt2Idx ].Latitude}, {points[ pt2Idx ].Longitude}) exceeds {match.Groups[ 3 ].Value} {match.Groups[ 4 ].Value.Trim()}";
    }

    private async Task HandleTimeoutExceptionAsync()
    {
        await SendMessage( ExpandedPhase,
                           $"Bing processing timed out after {RequestTimeout}",
                           true,
                           true,
                           LogLevel.Error );
    }

    private async Task HandleOtherRequestExceptionAsync( string mesg )
    {
        await SendMessage( ExpandedPhase,
                           $"Bing processing failed, reply was {mesg}",
                           true,
                           true,
                           LogLevel.Error );
    }

    private async Task HandleInvalidStatusCodeAsync( string description )
    {
        await SendMessage( ExpandedPhase,
                           $"Snap to road request failed, message was '{description}'",
                           true,
                           true,
                           LogLevel.Error );
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
                                                                         y => new Coordinate2(
                                                                             y.Coordinate.Latitude,
                                                                             y.Coordinate.Longitude ) ) )
                                                                    .ToList() );

            _processedChunks!.Add( snapResult );
        }
    }
}
