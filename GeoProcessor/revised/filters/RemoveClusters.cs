using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.GeoProcessor;

[BeforeAllImportFilter("Remove Clusters", 0)]
public class RemoveClusters : ImportFilter
{
    private double _maxClusterDiameter = GeoConstants.DefaultMaxClusterDiameterMeters;

    public RemoveClusters(
        ILoggerFactory? loggerFactory
    )
    :base( loggerFactory)
    {
    }

    public double MaximumClusterDiameter
    {
        get => _maxClusterDiameter;
        set => _maxClusterDiameter = value <= 0 ? GeoConstants.DefaultMaxClusterDiameterMeters: value;
    }

    public override List<ImportedRoute> Filter( List<ImportedRoute> input )
    {
        if( !input.Any() )
        {
            Logger?.LogInformation( "Nothing to filter" );
            return input;
        }

        var retVal = new List<ImportedRoute>();

        foreach( var route in input )
        {
            retVal.Add( FilterRoute(route) );
        }

        return retVal;
    }

    private ImportedRoute FilterRoute( ImportedRoute toFilter )
    {
        var retVal = new ImportedRoute() { RouteName = toFilter.RouteName };

        Coordinate2? clusterOrigin = null;

        foreach( var coordinate in toFilter.Coordinates )
        {
            if( clusterOrigin == null )
            {
                clusterOrigin = coordinate;
                retVal.Coordinates.Add( coordinate );

                continue;
            }

            var ptPair = new PointPair( clusterOrigin, coordinate );
            if( ptPair.GetDistance()*1000<= MaximumClusterDiameter)
                continue;

            retVal.Coordinates.Add( coordinate );
            clusterOrigin = coordinate;
        }

        return retVal;
    }
}
