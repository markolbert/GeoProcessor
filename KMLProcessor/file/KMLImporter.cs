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
    [Importer(ImportType.KML)]
    public class KMLImporter : FileHandler, IImport
    {
        public KMLImporter( AppConfig config, IJ4JLogger logger )
            : base( config, logger )
        {
            Type = KMLExtensions.GetTargetType<ImporterAttribute>(GetType())!.Type;
        }

        public ImportType Type { get; protected set; }

        public virtual async Task<List<KmlDocument>?> ImportAsync( string filePath, CancellationToken cancellationToken )
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

            return ProcessXDocumentAsync( xDoc );
        }

        protected List<KmlDocument>? ProcessXDocumentAsync( XDocument xDoc )
        {
            var coordElement = xDoc.Descendants()
                .SingleOrDefault(x => x.Name.LocalName == "coordinates");

            if (coordElement == null)
            {
                Logger.Error("Could not find 'coordinates' element in XDocument");
                return null;
            }

            var coordRaw = coordElement.Value.Replace("\t", "")
                .Replace("\n", "");

            var retVal = new KmlDocument(Configuration)
            {
                RouteName = xDoc.Root?.Descendants()
                                .FirstOrDefault(x => x.Name.LocalName.Equals("name", StringComparison.OrdinalIgnoreCase))?.Value
                            ?? "Unnamed Route",
            };

            LinkedListNode<Coordinate>? prevPoint = null;

            foreach (var coordText in coordRaw.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            {
                prevPoint = retVal.Points.Count == 0
                    ? retVal.Points.AddFirst(new Coordinate(coordText))
                    : retVal.Points.AddAfter(prevPoint!, new Coordinate(coordText));
            }

            return new List<KmlDocument> { retVal };
        }
    }
}