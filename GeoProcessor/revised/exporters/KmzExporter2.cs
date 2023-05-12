using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.GeoProcessor;

public class KmzExporter2 : KmlExporter2
{
    public KmzExporter2(
        ILoggerFactory? loggerFactory
    )
        : base( "kmz", loggerFactory )
    {
    }

    protected override async Task OutputMemoryStream( MemoryStream memoryStream )
    {
        var entryPath = ChangeFileExtension( FilePath, "kml" );

        await using var zipFile = new FileStream( FilePath, FileMode.Create );
        using var archive = new ZipArchive( zipFile, ZipArchiveMode.Create, true );
        var entry = archive.CreateEntry( entryPath );
        var zipStream = entry.Open();
        await memoryStream.CopyToAsync( zipStream );
        zipStream.Close();
    }
}
