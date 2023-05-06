using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GoogleApi.Entities.Maps.Roads.SnapToRoads.Request;
using GoogleApi.Entities.Maps.Roads.SnapToRoads.Response;
using GoogleApi;
using GoogleCoordinates = GoogleApi.Entities.Maps.Roads.Common;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.GeoProcessor;

[RouteProcessorAttribute2("Google")]
public class GoogleProcessor2 : RouteProcessor2
{
    public GoogleProcessor2(
        ILoggerFactory? loggerFactory = null
    )
        : base( null, loggerFactory )
    {
    }

    protected override async Task<ProcessRouteResult> ProcessRouteInternalAsync(
        List<ImportedRoute> importedRoutes,
        CancellationToken ctx
    )
    {
        var retVal = new ProcessRouteResult();

        foreach( var importedRoute in importedRoutes )
        {
            var request = new SnapToRoadsRequest
            {
                Interpolate = true,
                Key = ApiKey,
                Path = importedRoute.Coordinates.Select(c => new GoogleCoordinates.Coordinate(c.Latitude, c.Longitude))
            };

            SnapToRoadsResponse result;

            try
            {
                result = await GoogleMaps.Roads.SnapToRoad.QueryAsync(request, ctx);
            }
            catch (Exception e)
            {
                Logger?.LogError("Snap to road request failed. Message was '{mesg}'", e.Message);
                return ProcessRouteResult.Failed;
            }

            if (result == null)
            {
                Logger?.LogError("Snap to road request failed");
                return ProcessRouteResult.Failed;
            }
        }

        return retVal;
    }
}
