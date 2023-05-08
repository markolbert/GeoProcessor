using System.Xml.Linq;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.GeoProcessor;

public class GpxExporter : FileExporter
{
    public GpxExporter( 
        ILoggerFactory? loggerFactory 
    )
        : base( "gpx", loggerFactory )
    {
    }

    protected override XDeclaration GetXDeclaration() => new( "1.0", null, "yes" );
    
    protected override XElement GetXmlRoot()
    {
        XNamespace ns = "http://www.topografix.com/GPX/1/1";
        XNamespace nsXsd = "http://www.w3.org/2001/XMLSchema";
        XNamespace nsXsi = "http://www.w3.org/2001/XMLSchema-instance";
        XNamespace nsSchemaLocation = "http://www.topografix.com/GPX/1/1 http://www.topografix.com/GPX/1/1/gpx.xsd";

        var content = new object[]
        {
            new XAttribute( XNamespace.Xmlns + string.Empty, ns ),
            new XAttribute( XNamespace.Xmlns + "xsd", nsXsd ),
            new XAttribute( XNamespace.Xmlns + "xsi", nsXsi ),
            new XAttribute( "xsi:schemaLocation", nsSchemaLocation ),
            new XAttribute( "version", "1.1" )
        };

        var retVal = new XElement( "gpx", content );

        return retVal;
    }

    protected override XElement GetRouteElement( ExportedRoute route )
    {
        var retVal = new XElement( GeoConstants.TrackName );

        if( !string.IsNullOrEmpty(route.RouteName  ))
            retVal.Add( new XElement( GeoConstants.RouteName ), route.RouteName );

        if( !string.IsNullOrEmpty( route.Description ) )
            retVal.Add( new XElement( GeoConstants.DescriptionName ), route.Description );

        var trkSegment = new XElement( GeoConstants.TrackSegmentName );
        retVal.Add( trkSegment );

        foreach( var point in route.Points )
        {
            trkSegment.Add( GetPointElement( point ) );
        }

        return retVal;
    }

    private static XElement GetPointElement( Coordinate2 point )
    {
        var retVal = new XElement( GeoConstants.TrackPointName );

        retVal.Add(new XAttribute(GeoConstants.LatitudeName, point.Latitude)  );
        retVal.Add(new XAttribute(GeoConstants.LongitudeName, point.Longitude));

        if( point.Elevation.HasValue )
            retVal.Add( new XElement( GeoConstants.ElevationName, point.Elevation.Value ) );

        if( point.Timestamp.HasValue )
            retVal.Add( new XElement( GeoConstants.TimeName, point.Timestamp.Value.ToString( "o" ) ) );

        if( !string.IsNullOrEmpty( point.Description ) )
            retVal.Add( new XElement( GeoConstants.DescriptionName, point.Description ) );

        return retVal;
    }
}
