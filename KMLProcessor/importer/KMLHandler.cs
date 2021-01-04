using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using J4JSoftware.Logging;

namespace J4JSoftware.KMLProcessor
{
    public class KMLHandler : FileHandler, IImportExport
    {
        public KMLHandler( AppConfig config, IJ4JLogger logger )
            : base( config, logger )
        {
        }

        public FileType Type => FileType.KML;

        public async Task<KmlDocument?> ImportAsync( string filePath, CancellationToken cancellationToken )
        {
            if (!File.Exists(filePath))
            {
                Logger.Error<string>("File '{0}' does not exist", filePath);
                return null;
            }

            FilePath = filePath;
            XDocument? xDoc = null;

            try
            {
                using var readStream = File.OpenText(FilePath);
                xDoc = await XDocument.LoadAsync(readStream, LoadOptions.None, cancellationToken);
            }
            catch (Exception e)
            {
                Logger.Error<string, string>("Could not load file '{0}', exception was '{1}'", FilePath, e.Message);
                return null;
            }

            var coordElement = xDoc.Descendants()
                .SingleOrDefault(x => x.Name.LocalName == "coordinates");

            if (coordElement == null)
            {
                Logger.Error("Could not find 'coordinates' element in XDocument");
                return null;
            }

            var coordRaw = coordElement.Value.Replace("\t", "")
                .Replace("\n", "");

            var retVal = new KmlDocument( Configuration )
            {
                RouteName = xDoc.Root?.Descendants()
                                .FirstOrDefault(x=>x.Name.LocalName.Equals("name", StringComparison.OrdinalIgnoreCase))?.Value
                            ?? "Unnamed Route",
            };

            LinkedListNode<Coordinate>? prevPoint = null;

            foreach (var coordText in coordRaw.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            {
                prevPoint = retVal.Points.Count == 0
                    ? retVal.Points.AddFirst(new Coordinate(coordText))
                    : retVal.Points.AddAfter(prevPoint!, new Coordinate(coordText));

                if (!cancellationToken.IsCancellationRequested)
                    continue;

                Logger.Information("File load cancelled");
                return null;
            }

            return retVal;
        }

        public async Task<bool> ExportAsync(
            KmlDocument kDoc,
            string filePath,
            CancellationToken cancellationToken )
        {
            if( kDoc.Points.Count == 0 )
            {
                Logger.Error( "Cannot save empty KmlDocument" );
                return false;
            }

            var xDoc = new XDocument( new XDeclaration( "1.0", "UTF-8", "no" ) );

            var ns = XNamespace.Get( "http://www.opengis.net/kml/2.2" );
            var root = new XElement( "kml" );
            root.Add( new XAttribute( XNamespace.Xmlns + "gx", "http://www.google.com/kml/ext/2.2" ) );
            root.Add( new XAttribute( XNamespace.Xmlns + "kml", "http://www.opengis.net/kml/2.2" ) );
            root.Add( new XAttribute( XNamespace.Xmlns + "atom", "http://www.w3.org/2005/Atom" ) );

            xDoc.Add( root );

            var document = new XElement( "Document" );
            root.Add( document );
            document.Add( new XElement( "name", kDoc.RouteName ) );

            var styleMap = new XElement( "StyleMap" );
            styleMap.Add( new XAttribute( "id", "standard" ) );
            document.Add( styleMap );

            var normalPair = new XElement( "Pair" );
            normalPair.Add( new XElement( "key", "normal" ) );
            normalPair.Add( new XElement( "styleUrl", "#regular" ) );
            styleMap.Add( normalPair );

            var highlightPair = new XElement( "Pair" );
            highlightPair.Add( new XElement( "key", "highlight" ) );
            highlightPair.Add( new XElement( "styleUrl", "#highlight" ) );
            styleMap.Add( highlightPair );

            var normalStyle = new XElement( "Style" );
            normalStyle.Add( new XAttribute( "id", "regular" ) );
            document.Add( normalStyle );

            var normalLine = new XElement( "LineStyle" );
            normalStyle.Add( normalLine );
            normalLine.Add( new XElement( "color", KMLExtensions.ToAbgrHex(kDoc.RouteColor) ) );
            normalLine.Add( new XElement( "width", kDoc.RouteWidth ) );

            var highlightStyle = new XElement( "Style" );
            highlightStyle.Add( new XAttribute( "id", "highlight" ) );
            document.Add( highlightStyle );

            var highlightLine = new XElement( "LineStyle" );
            highlightStyle.Add( highlightLine );
            highlightLine.Add( new XElement( "color", KMLExtensions.ToAbgrHex(kDoc.RouteHighlightColor) ) );
            highlightLine.Add( new XElement( "width", kDoc.RouteWidth ) );

            var placeMark = new XElement( "Placemark" );
            document.Add( placeMark );
            placeMark.Add( new XElement( "name", kDoc.RouteName ) );
            placeMark.Add( new XElement( "styleUrl", "#standard" ) );

            var lineString = new XElement( "LineString" );
            placeMark.Add( lineString );
            lineString.Add( new XElement( "extrude", 1 ) );
            lineString.Add( new XElement( "tesselate", 1 ) );

            var coordinates = new XElement( "coordinates" );
            lineString.Add( coordinates );

            var sb = new StringBuilder();
            sb.AppendLine();

            foreach( var point in kDoc.Points )
                // having NO SPACES between these three arguments is INCREDIBLY IMPORTANT.
                // the Google Earth importer parses based on spaces (but ignores tabs, linefeeds & newlines)
                // also note that LONGITUDE is emitted FIRST!!!
                sb.AppendLine( $"\t\t\t{point.Longitude},{point.Latitude},0 " );

            coordinates.Value = sb.ToString();

            await using var writeStream = File.CreateText( filePath );

            await xDoc!.SaveAsync( writeStream, SaveOptions.None, cancellationToken );

            await writeStream.FlushAsync();
            writeStream.Close();

            return true;
        }
    }
}