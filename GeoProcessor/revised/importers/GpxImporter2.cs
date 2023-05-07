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

            var folder = new ImportedRoute( trkName, new List<Coordinate2>() );

            foreach (var trackSeg in track.GetNamedDescendants( GeoConstants.TrackSegmentName))
            {
                // Garmin seems to send a track segment composed of ancient text messages
                // that you haven't yet deleted from the InReach device. We try to filter these
                // out here.

                // if any of the track point elements have a non-null desc field (which is where
                // the text of a message is stored), then ignore that entire segment
                var trackPoints = trackSeg.GetNamedDescendants( GeoConstants.TrackPointName );

                // remove all the track points that contain messages
                trackPoints = trackPoints.Where( x => string.IsNullOrEmpty( x.GetFirstDescendantValue( GeoConstants.MessageName ) ) )
                                         .ToList();

                // ignore any set of track points whose elements are widely separated in time
                var allSeparated = true;
                DateTime? prevDt = null;
                var minTs = TimeSpan.FromDays( 1 );

                foreach( var trackPoint in trackPoints )
                {
                    var curTimeValue = trackPoint.GetFirstDescendantValue( GeoConstants.TimeName );

                    DateTime? curDt = null;

                    if( !string.IsNullOrEmpty( curTimeValue ) && DateTime.TryParse( curTimeValue, out var temp ) )
                        curDt = temp;

                    if( prevDt != null )
                    {
                        allSeparated &= curDt == null || curDt - prevDt >= minTs;
                        if( !allSeparated )
                            break;
                    }
                    
                    prevDt = curDt;
                }

                if( allSeparated )
                    trackPoints.Clear();

                // ignore any set of track points which is empty or has only one member
                if (trackPoints.Count < 2)
                    continue;

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

                    folder.Coordinates.Add( coordinate );
                }

                if (folder.Coordinates.Any())
                    retVal.Add(folder);
            }
        }

        return retVal;
    }
}
