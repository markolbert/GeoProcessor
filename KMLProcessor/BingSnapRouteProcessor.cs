using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BingMapsRESTToolkit;
using J4JSoftware.Logging;

namespace J4JSoftware.KMLProcessor
{
    public class BingSnapRouteProcessor : SnapRouteProcessor
    {
        public BingSnapRouteProcessor( 
            IAppConfig config,
            IJ4JLogger logger
            )
        :base(config, logger)
        {
        }

        public override Distance MaxSeparation { get; } = new Distance( UnitTypes.Kilometers, 2.5 );
        public override int PointsPerRequest { get; } = 100;

        protected override async Task<List<Coordinate>?> ExecuteRequestAsync( List<Coordinate> chunk, CancellationToken cancellationToken )
        {
            var request = new SnapToRoadRequest
            {
                BingMapsKey = Configuration.BingMapsKey,
                IncludeSpeedLimit = false,
                IncludeTruckSpeedLimit = false,
                Interpolate = true,
                SpeedUnit = SpeedUnitType.MPH,
                TravelMode = TravelModeType.Driving,
                Points = chunk.Select(p => p.ToBingMapsCoordinate()).ToList()
            };

            var result = await request.Execute();

            if (result.StatusCode != 200)
            {
                Logger.Error<string>("Snap to road request failed, message was '{0}'", result.StatusDescription);
                return null;
            }

            var retVal = new List<Coordinate>();

            foreach (var resourceSet in result.ResourceSets)
            {
                var snapResponses = resourceSet.Resources
                    .Where(r => r is SnapToRoadResponse)
                    .Cast<SnapToRoadResponse>()
                    .ToList();

                if (!snapResponses.Any())
                {
                    Logger.Error("Snap to request did not return usable results");
                    return null;
                }

                foreach (var snapResponse in snapResponses)
                {
                    retVal.AddRange(snapResponse.SnappedPoints
                        .Select(p => new Coordinate(p))
                    );
                }
            }

            return retVal;
        }
    }
}
