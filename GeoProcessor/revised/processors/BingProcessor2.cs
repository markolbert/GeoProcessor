using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BingMapsRESTToolkit;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.GeoProcessor;

[ RouteProcessorAttribute2( "Bing" ) ]
public class BingProcessor2 : RouteProcessor2
{
    private ProcessRouteResult? _result;
    private string? _folderName;

    public BingProcessor2(
        ILoggerFactory? loggerFactory
    )
        : base( null,
                loggerFactory,
                new InterpolatePoints( loggerFactory ) { MaximumPointSeparation = 2.5 } )
    {
    }

    protected override async Task<ProcessRouteResult> ProcessRouteInternalAsync(
        List<ImportedRoute> importedRoutes,
        CancellationToken ctx
    )
    {
        _result = new ProcessRouteResult();

        foreach( var importedRoute in importedRoutes )
        {
            _folderName = importedRoute.RouteName;

            var request = new SnapToRoadRequest
            {
                BingMapsKey = ApiKey,
                IncludeSpeedLimit = false,
                IncludeTruckSpeedLimit = false,
                Interpolate = true,
                SpeedUnit = SpeedUnitType.MPH,
                TravelMode = TravelModeType.Driving,
                Points = importedRoute.Coordinates
                                      .Select( p => new BingMapsRESTToolkit.Coordinate( p.Latitude, p.Longitude ) )
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
                await HandleOtherRequestExceptionAsync( ex );
                continue;
            }

            if( result.StatusCode != 200 )
            {
                await HandleInvalidStatusCodeAsync( result.StatusDescription );
                continue;
            }

            foreach( var resourceSet in result.ResourceSets )
            {
                await ProcessResourceSetAsync( resourceSet );
            }
        }

        return _result;
    }

    private async Task HandleTimeoutExceptionAsync()
    {
        await SendMessage( ExpandedPhase,
                           $"Bing processing timed out after {RequestTimeout.ToString()}",
                           true,
                           LogLevel.Error );

        _result!.AddResult( new ExportedRoute( _folderName!, null, SnapProcessStatus.TimeOut ) );
    }

    private async Task HandleOtherRequestExceptionAsync( Exception ex )
    {
        await SendMessage( ExpandedPhase,
                           $"Bing processing failed, reply was {ex.Message}",
                           true,
                           LogLevel.Error );

        _result!.AddResult( new ExportedRoute( _folderName!, null, SnapProcessStatus.OtherRequestFailure ) );
    }

    private async Task HandleInvalidStatusCodeAsync( string description )
    {
        await SendMessage( ExpandedPhase,
                           $"Snap to road request failed, message was '{description}'",
                           true,
                           LogLevel.Error );

        _result!.AddResult( new ExportedRoute( _folderName!, null, SnapProcessStatus.InvalidStatusCode ) );
    }

    private async Task ProcessResourceSetAsync( ResourceSet resourceSet )
    {
        var snapResponses = resourceSet.Resources
                                       .Where( r => r is SnapToRoadResponse )
                                       .Cast<SnapToRoadResponse>()
                                       .ToList();

        if( !snapResponses.Any() )
        {
            await SendMessage( ExpandedPhase,
                               "Snap to request did not return usable results",
                               true,
                               LogLevel.Error );

            _result!.AddResult( new ExportedRoute( _folderName!, null, SnapProcessStatus.NoResultsReturned ) );
        }
        else
        {
            var snapResult = new ExportedRoute( _folderName!,
                                                snapResponses.SelectMany( x => x.SnappedPoints.Select(
                                                                              y => new Coordinate2(
                                                                                  y.Coordinate.Latitude,
                                                                                  y.Coordinate.Longitude ) ) )
                                                             .ToList(),
                                                SnapProcessStatus.IsValid );

            _result!.AddResult( snapResult );
        }
    }
}
