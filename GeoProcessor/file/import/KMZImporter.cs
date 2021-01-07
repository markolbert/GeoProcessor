using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using J4JSoftware.Logging;

namespace J4JSoftware.GeoProcessor
{
    [Importer(ImportType.KMZ)]
    public class KMZImporter : KMLImporter
    {
        public KMZImporter( IGeoConfig config, IJ4JLogger? logger = null )
            : base( config, logger )
        {
            Type = GeoExtensions.GetTargetType<ImporterAttribute>(GetType())!.Type;
        }

        public override async Task<List<PointSet>?> ImportAsync( string filePath, CancellationToken cancellationToken )
        {
            if (!File.Exists(filePath))
            {
                Logger?.Error<string>("File '{0}' does not exist", filePath);
                return null;
            }

            XDocument? xDoc = null;

            try
            {
                using var zipArchive = ZipFile.OpenRead( filePath );

                var kmlEntry = zipArchive.Entries
                    .FirstOrDefault( x => x.FullName.EndsWith( ".kml", StringComparison.OrdinalIgnoreCase ) );

                if( kmlEntry == null )
                {
                    Logger?.Error<string>("File '{0}' does not contain any KML files", filePath);
                    return null;
                }

                xDoc = await XDocument.LoadAsync( kmlEntry.Open(), LoadOptions.None, cancellationToken );
            }
            catch (Exception e)
            {
                Logger?.Error<string, string>("Could not load file '{0}', exception was '{1}'", filePath, e.Message);
                return null;
            }

            return ProcessXDocumentAsync( xDoc );
        }
    }
}