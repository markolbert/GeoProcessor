using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.GeoProcessor;

public abstract class FileImporter : Importer, IFileImporter
{
    protected FileImporter(
        string? mesgPrefix = null,
        ILoggerFactory? loggerFactory = null
    )
    :base( mesgPrefix, loggerFactory )
    {
        var type = GetType();

        var fileType = type.GetCustomAttribute<FileTypeAttribute>();
        if( fileType == null || string.IsNullOrEmpty(fileType.FileType  ) )
        {
            Logger?.LogCritical( "{type} is not decorated with a valid {fileType}", type, typeof( FileTypeAttribute ) );
            throw new ArgumentException( $"{type} is not decorated with a valid {typeof( FileTypeAttribute )}" );
        }

        FileType = fileType.FileType;
    }

    public string FileType { get; }
    public bool LineStringsOnly { get; set; }

    protected async Task<XDocument?> OpenXmlFileAsync( string filePath, CancellationToken ctx )
    {
        XDocument? retVal;

        try
        {
            using var readStream = File.OpenText(filePath);
            retVal = await XDocument.LoadAsync(readStream, LoadOptions.None, ctx);
        }
        catch (Exception e)
        {
            Logger?.LogError("Could not load file '{path}', exception was '{mesg}'", filePath, e.Message);
            return null;
        }

        return retVal;
    }
}
