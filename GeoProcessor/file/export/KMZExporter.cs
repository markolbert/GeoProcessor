using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using J4JSoftware.Logging;

namespace J4JSoftware.GeoProcessor
{
    [Exporter(ExportType.KMZ)]
    public class KMZExporter : KMLExporter
    {
        public KMZExporter(IExportConfig config, IJ4JLogger? logger = null )
            : base(config, logger)
        {
            Type = GeoExtensions.GetTargetType<ExporterAttribute>(GetType())!.Type;
        }

        public override async Task<bool> ExportAsync( PointSet pointSet, int docIndex, CancellationToken cancellationToken)
        {
            var xDoc = CreateXDocument( pointSet );

            if( xDoc == null )
                return false;

            var exportConfig = (IExportConfig) Configuration;

            string? curFilePath = string.Empty;

            try
            {
                curFilePath = exportConfig.OutputFile.GetPath(docIndex);

                await using var fileStream = File.Create( curFilePath );
                using var archive = new ZipArchive( fileStream, ZipArchiveMode.Create, true );

                var kmlEntry = archive.CreateEntry( $"{exportConfig.OutputFile.FileNameWithoutExtension}.kml" );
                await using var kmlStream = kmlEntry.Open();

                await xDoc!.SaveAsync( kmlStream, SaveOptions.None, cancellationToken );

                await fileStream.FlushAsync( cancellationToken );

                Logger?.Information<string>("Wrote file '{0}'", curFilePath);
            }
            catch( Exception e )
            {
                Logger?.Information<string, string>( 
                    "Export to file '{0}' failed, message was '{1}'", 
                    curFilePath,
                    e.Message );

                return false;
            }

            return true;
        }
    }
}