using System;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using J4JSoftware.Logging;

namespace J4JSoftware.KMLProcessor
{
    [Exporter(ExportType.KMZ)]
    public class KMZExporter : KMLExporter, IExport
    {
        public KMZExporter(AppConfig config, IJ4JLogger logger)
            : base(config, logger)
        {
            Type = KMLExtensions.GetTargetType<ExporterAttribute>(GetType())!.Type;
        }

        public override async Task<bool> ExportAsync( KmlDocument kDoc, int docIndex, CancellationToken cancellationToken)
        {
            var xDoc = await CreateXDocument( kDoc, cancellationToken );

            if( xDoc == null )
                return false;

            string? curFilePath = string.Empty;

            try
            {
                curFilePath = GetNumberedFilePath(docIndex);

                await using var fileStream = File.Create( curFilePath );
                using var archive = new ZipArchive( fileStream, ZipArchiveMode.Create, true );

                var kmlEntry = archive.CreateEntry( $"{Path.GetFileNameWithoutExtension( FilePath )}.kml" );
                await using var kmlStream = kmlEntry.Open();

                await xDoc!.SaveAsync( kmlStream, SaveOptions.None, cancellationToken );

                await fileStream.FlushAsync( cancellationToken );

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
    }
}