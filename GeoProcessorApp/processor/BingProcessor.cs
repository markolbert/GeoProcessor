using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BingMapsRESTToolkit;
using J4JSoftware.Logging;

namespace J4JSoftware.GeoProcessor
{
    [RouteProcessor(ProcessorType.Bing)]
    public class BingProcessor : CloudRouteProcessor
    {
        public BingProcessor(
            AppConfig config,
            IJ4JLogger logger
        )
            : base( config, logger )
        {
            Type = KMLExtensions.GetTargetType<RouteProcessorAttribute>(GetType())!.Type;
        }

        public ProcessorType Type { get; }

        protected override async Task<List<Coordinate>?> ExecuteRequestAsync( 
            List<Coordinate> coordinates,
            CancellationToken cancellationToken )
        {
            var request = new SnapToRoadRequest
            {
                BingMapsKey = APIKey,
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
                Logger.Error<string>( "Snap to road request failed, message was '{0}'", result.StatusDescription );
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
                    Logger.Error( "Snap to request did not return usable results" );
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
}