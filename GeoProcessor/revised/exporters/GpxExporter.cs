using System.Collections.Generic;
using System.Linq;
using J4JSoftware.GeoProcessor.Gpx;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.GeoProcessor;

public class GpxExporter : FileExporter<Root>
{
    public GpxExporter( 
        ILoggerFactory? loggerFactory 
    )
        : base( "gpx", loggerFactory )
    {
    }
    protected override void InitializeColorPicker() => RouteColorPicker = GeoExtensions.RouteColorPicker;

    protected override Root GetRootObject(List<IImportedRoute> routes)
    {
        var retVal = new Root()
        {
            Creator = "https://www.jumpforjoysoftware.com",
            SchemaLocation = "http://www.topografix.com/GPX/1/1 http://www.topografix.com/GPX/1/1/gpx.xsd",
            Version = "1.1"
        };

        var tracks = new List<Track>();

        foreach( var route in routes )
        {
            var track = new Track { Description = route.Description, Name = route.RouteName };
            tracks.Add( track );

            track.TrackPoints = route.Select( x => new TrackPoint
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
