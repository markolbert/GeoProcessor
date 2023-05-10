using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.GeoProcessor;

[BeforeUserFilters(DefaultFilterName, 10)]
public class RemoveClusters : ImportFilter
{
    public const string DefaultFilterName = "Remove Clusters";

    private Distance2 _maxClusterDiameter = new( UnitType.Meters, GeoConstants.DefaultMaxClusterDiameterMeters );

    public RemoveClusters(
        ILoggerFactory? loggerFactory
    )
    :base( loggerFactory)
    {
    }

    public Distance2 MaximumClusterDiameter
    {
        get => _maxClusterDiameter;

        set =>
            _maxClusterDiameter = value.Value <= 0
                ? new Distance2( UnitType.Meters, GeoConstants.DefaultMaxClusterDiameterMeters )
                : value;
    }

    public override List<IImportedRoute> Filter( List<IImportedRoute> input )
    {
        if( !input.Any() )
        {
            Logger?.LogInformation( "Nothing to filter" );
            return input;
        }

        var retVal = new List<IImportedRoute>();

        foreach( var route in input )
        {
            retVal.Add( FilterRoute(route) );
        }

        return retVal;
    }

    private IImportedRoute FilterRoute( IImportedRoute toFilter )
    {
        var retVal = new ImportedRoute() { RouteName = toFilter.RouteName };

        Coordinate2? clusterOrigin = null;

        foreach( var coordinate in toFilter )
        {
            if( clusterOrigin == null )
            {
                clusterOrigin = coordinate;
                retVal.Points.Add( coordinate );

                continue;
            }

            var ptPair = new PointPair( clusterOrigin, coordinate );
            if( ptPair.GetDistance() <= MaximumClusterDiameter )
                continue;

            retVal.Points.Add( coordinate );
            clusterOrigin = coordinate;
        }

        return retVal;
    }
}
