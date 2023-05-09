using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.GeoProcessor;

public class GpxExporter : FileExporter<GpxDoc>
{
    public GpxExporter( 
        ILoggerFactory? loggerFactory 
    )
        : base( "gpx", loggerFactory )
    {
    }

    protected override GpxDoc GetDocumentObject(IEnumerable<ExportedRoute> routes)
    {
        var retVal = new GpxDoc()
        {
            Xsi = "http://www.w3.org/2001/XMLSchema-instance",
            SchemaLocation = "http://www.topografix.com/GPX/1/1 http://www.topografix.com/GPX/1/1/gpx.xsd",
            Version = "1.1"
        };

        var tracks = new List<GpxTrack>();

        foreach( var route in routes )
        {
            var track = new GpxTrack { Description = route.Description, Name = route.RouteName };
            tracks.Add( track );

            track.TrackPoints = route.Points.Select( x => new GpxTrackPoint
                                      {
                                          Latitude = x.Latitude,
                                          Longitude = x.Longitude,
                                          Timestamp = x.Timestamp,
                                          Description = x.Description,
                                          Elevation = x.Elevation
                                      } )
                                     .ToArray();
        }

        retVal.Tracks = tracks.ToArray();

        return retVal;
    }
}
