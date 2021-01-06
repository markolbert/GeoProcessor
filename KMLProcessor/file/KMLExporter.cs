using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using J4JSoftware.Logging;
using J4JSoftware.VisualUtilities;

namespace J4JSoftware.KMLProcessor
{
    [Exporter(ExportType.KML)]
    public class KMLExporter : FileHandler, IExport
    {
        public KMLExporter(AppConfig config, IJ4JLogger logger)
            : base(config, logger)
        {
            Type = KMLExtensions.GetTargetType<ExporterAttribute>(GetType())!.Type;
        }

        public ExportType Type { get; protected set; }

        public virtual async Task<bool> ExportAsync( KmlDocument kDoc, int docIndex, CancellationToken cancellationToken)
        {
            var xDoc = CreateXDocument( kDoc );

            if( xDoc == null )
                return false;

            string? curFilePath = string.Empty;

            try
            {
                curFilePath = GetNumberedFilePath( docIndex );

                await using var writeStream = File.CreateText(curFilePath);
                await xDoc!.SaveAsync( writeStream, SaveOptions.None, cancellationToken );
                await writeStream.FlushAsync();
                writeStream.Close();

                Logger.Information<string>("Wrote file '{0}'", curFilePath);
            }
            catch( Exception e )
            {
                Logger.Information<string, string>( 
                    "Export to file '{0}' failed, message was '{1}'", 
                    curFilePath,
                    e.Message );

                return false;
            }

            return true;
        }

        protected XDocument? CreateXDocument( KmlDocument kDoc )
        {
            if (kDoc.Points.Count == 0)
            {
                Logger.Error("Cannot save empty KmlDocument");
                return null;
            }

            FilePath = Configuration.OutputFile!;

            var xDoc = new XDocument(new XDeclaration("1.0", "UTF-8", "no"));

            var ns = XNamespace.Get("http://www.opengis.net/kml/2.2");
            var root = new XElement("kml");
            root.Add(new XAttribute(XNamespace.Xmlns + "gx", "http://www.google.com/kml/ext/2.2"));
            root.Add(new XAttribute(XNamespace.Xmlns + "kml", "http://www.opengis.net/kml/2.2"));
            root.Add(new XAttribute(XNamespace.Xmlns + "atom", "http://www.w3.org/2005/Atom"));

            xDoc.Add(root);

            var document = new XElement("Document");
            root.Add(document);
            document.Add(new XElement("name", kDoc.RouteName));

            var styleMap = new XElement("StyleMap");
            styleMap.Add(new XAttribute("id", "standard"));
            document.Add(styleMap);

            var normalPair = new XElement("Pair");
            normalPair.Add(new XElement("key", "normal"));
            normalPair.Add(new XElement("styleUrl", "#regular"));
            styleMap.Add(normalPair);

            var highlightPair = new XElement("Pair");
            highlightPair.Add(new XElement("key", "highlight"));
            highlightPair.Add(new XElement("styleUrl", "#highlight"));
            styleMap.Add(highlightPair);

            var normalStyle = new XElement("Style");
            normalStyle.Add(new XAttribute("id", "regular"));
            document.Add(normalStyle);

            var normalLine = new XElement("LineStyle");
            normalStyle.Add(normalLine);
            normalLine.Add(new XElement("color", kDoc.RouteColor.ToAbgrHex()));
            normalLine.Add(new XElement("width", kDoc.RouteWidth));

            var highlightStyle = new XElement("Style");
            highlightStyle.Add(new XAttribute("id", "highlight"));
            document.Add(highlightStyle);

            var highlightLine = new XElement("LineStyle");
            highlightStyle.Add(highlightLine);
            highlightLine.Add(new XElement("color", kDoc.RouteHighlightColor.ToAbgrHex()));
            highlightLine.Add(new XElement("width", kDoc.RouteWidth));

            var placeMark = new XElement("Placemark");
            document.Add(placeMark);
            placeMark.Add(new XElement("name", kDoc.RouteName));
            placeMark.Add(new XElement("styleUrl", "#standard"));

            var lineString = new XElement("LineString");
            placeMark.Add(lineString);
            lineString.Add(new XElement("extrude", 1));
            lineString.Add(new XElement("tesselate", 1));

            var coordinates = new XElement("coordinates");
            lineString.Add(coordinates);

            var sb = new StringBuilder();
            sb.AppendLine();

            foreach (var point in kDoc.Points)
                // having NO SPACES between these three arguments is INCREDIBLY IMPORTANT.
                // the Google Earth importer parses based on spaces (but ignores tabs, linefeeds & newlines)
                // also note that LONGITUDE is emitted FIRST!!!
                sb.AppendLine($"\t\t\t{point.Longitude},{point.Latitude},0 ");

            coordinates.Value = sb.ToString();

            return xDoc;
        }
    }
}