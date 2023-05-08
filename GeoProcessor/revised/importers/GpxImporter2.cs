using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using J4JSoftware.GeoProcessor.RouteBuilder;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.GeoProcessor;

[ImportFileType("gpx")]
public class GpxImporter2 : FileImporter
{
    public GpxImporter2(
        ILoggerFactory? loggerFactory = null
    )
        : base( null, loggerFactory )
    {
    }

    protected override async Task<List<ImportedRoute>> ImportInternalAsync( DataToImportBase toImport, CancellationToken ctx )
    {
        var retVal = new List<ImportedRoute>();

        if ( toImport is not FileToImport fileToImport )
        {
            Logger?.LogError( "Expected a {correct} but got a {incorrect} instead",
                              typeof( FileToImport ),
                              toImport.GetType() );

            return retVal;
        }

        var doc = await OpenXmlFileAsync( fileToImport.FilePath, ctx );
        if( doc == null )
            return retVal;

        foreach (var track in doc.Descendants().Where(x => x.IsNamedElement( GeoConstants.TrackName)))
        {
            var trkName = track.GetFirstDescendantValue( GeoConstants.RouteName) ?? "Unnamed Route";
            var trkDesc = track.GetFirstDescendantValue(GeoConstants.RouteName) ?? string.Empty;

            var importedRoute = new ImportedRoute( new List<Coordinate2>() )
            {
                RouteName = trkName, Description = trkDesc
            };

            foreach (var trackSeg in track.GetNamedDescendants( GeoConstants.TrackSegmentName))
            {
                var trackPoints = trackSeg.GetNamedDescendants(GeoConstants.TrackPointName);

                foreach ( var trackPoint in trackPoints )
                {
                    if( !trackPoint.TryParseAttribute<double>( GeoConstants.LatitudeName, out var latitude )
                    || !trackPoint.TryParseAttribute<double>( GeoConstants.LongitudeName, out var longitude ) )
                    {
                        Logger?.LogWarning("Couldn't parse latitude or longitude text");
                        continue;
                    }

                    var coordinate = new Coordinate2( latitude, longitude );

                    if( trackPoint.TryParseFirstDescendantValue<double>( GeoConstants.ElevationName, out var elevation ) )
                        coordinate.Elevation = elevation;

                    if (trackPoint.TryParseFirstDescendantValue<DateTime>( GeoConstants.TimeName, out var timestamp))
                        coordinate.Timestamp= timestamp;

                    coordinate.Description = trackPoint.GetFirstDescendantValue( GeoConstants.DescriptionName );

                    importedRoute.Points.Add( coordinate );
                }

                if (importedRoute.Points.Any())
                    retVal.Add(importedRoute);
            }
        }

        return retVal;
    }
}
