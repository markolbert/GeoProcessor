using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using J4JSoftware.Logging;

namespace J4JSoftware.KMLProcessor
{
    [Importer(ImportType.GPX)]
    public class GPXImporter : FileHandler, IImport
    {
        public GPXImporter( AppConfig config, IJ4JLogger logger )
            : base( config, logger )
        {
            Type = KMLExtensions.GetTargetType<ImporterAttribute>( GetType() )!.Type;
        }

        public ImportType Type { get; }

        public async Task<List<KmlDocument>?> ImportAsync( string filePath, CancellationToken cancellationToken )
        {
            if( !File.Exists( filePath ) )
            {
                Logger.Error<string>( "File '{0}' does not exist", filePath );
                return null;
            }

            FilePath = filePath;
            XDocument? xDoc = null;

            try
            {
                using var readStream = File.OpenText( FilePath );
                xDoc = await XDocument.LoadAsync( readStream, LoadOptions.None, cancellationToken );
            }
            catch( Exception e )
            {
                Logger.Error<string, string>( "Could not load file '{0}', exception was '{1}'", FilePath, e.Message );
                return null;
            }

            var retVal = new List<KmlDocument>();

            foreach( var track in xDoc.Descendants()
                .Where( x => x.Name.LocalName.Equals( "trk", StringComparison.OrdinalIgnoreCase ) ) )
            {
                var trkName =
                    track.Descendants().SingleOrDefault(
                        x => x.Name.LocalName.Equals( "name", StringComparison.OrdinalIgnoreCase ) )?.Value
                    ?? "Unnamed Route";

                var segNum = 0;

                foreach( var trackSeg in track.Descendants()
                    .Where( x => x.Name.LocalName.Equals( "trkSeg", StringComparison.OrdinalIgnoreCase ) ) )
                {
                    var curDoc = new KmlDocument( Configuration )
                    {
                        RouteName = segNum == 0 ? trkName : $"{trkName}-{segNum + 1}"
                    };

                    retVal.Add( curDoc );

                    LinkedListNode<Coordinate>? prevPoint = null;

                    foreach( var point in trackSeg.Descendants()
                        .Where( x =>
                            x.Name.LocalName.Equals( "tkpt", StringComparison.OrdinalIgnoreCase ) ) )
                    {
                        if( !ValidateDouble( point, "lon", "longitude", out var longitude ) )
                            continue;

                        if( !ValidateDouble( point, "lat", "latitude", out var latitude ) )
                            continue;
                        prevPoint = curDoc.Points.Count == 0
                            ? curDoc.Points.AddFirst( new Coordinate( latitude, longitude ) )
                            : curDoc.Points.AddAfter( prevPoint!, new Coordinate( latitude, longitude ) );

                        if( !cancellationToken.IsCancellationRequested )
                            continue;

                        Logger.Information( "File load cancelled" );
                        return null;
                    }

                    segNum++;
                }
            }

            return retVal;
        }

        private bool ValidateDouble( XElement point, string attrName, string name, out double result )
        {
            result = 99999;

            var text = point.Attribute(attrName)?.Value;

            if (string.IsNullOrEmpty(text))
            {
                Logger.Error("Missing longitude value");
                return false;
            }

            if (!double.TryParse(text!, out var retVal))
            {
                Logger.Error<string, string>("Unparseable {0} value '{1}'", name, text);
                return false;
            }

            result = retVal;

            return true;
        }
    }
}