using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic.FileIO;
using Microsoft.Win32.SafeHandles;

namespace J4JSoftware.GeoProcessor;

public abstract class FileExporter<TDoc> : Exporter, IFileExporter
    where TDoc : class
{
    private string _filePath = string.Empty;

    protected FileExporter(
        string fileType,
        ILoggerFactory? loggerFactory
    )
        : base( loggerFactory )
    {
        FileType = fileType.Length > 0 && fileType[ 0 ] == '.' ? fileType[ 1.. ] : fileType;
    }

    public string FileType { get; }

    public string FilePath
    {
        get => _filePath;
        set => _filePath = ChangeFileExtension( value, FileType );
    }

    protected string ChangeFileExtension( string filePath, string extension )
    {
        var dirPath = Path.GetDirectoryName( filePath ) ?? string.Empty;
        var noExt = Path.GetFileNameWithoutExtension( filePath );

        return string.IsNullOrEmpty( extension )
            ? Path.Combine( dirPath, noExt )
            : Path.Combine( dirPath, $"{noExt}.{extension}" );
    }

    public Func<IImportedRoute, int, Color>? RouteColorPicker { get; set; }
    public Func<IImportedRoute, int, int>? RouteWidthPicker { get; set; }

    protected override async Task<bool> ExportInternalAsync( List<IImportedRoute> routes, CancellationToken ctx )
    {
        if( string.IsNullOrEmpty( FilePath ) )
        {
            Logger?.LogError("Export file is undefined");
            return false;
        }

        InitializeColorPicker();
        InitializeWidthPicker();

        var docObject = GetRootObject( routes );
        var serializer = new XmlSerializer( typeof( TDoc ) );
        
        try
        {
            var memoryStream = new MemoryStream();
            var writer = XmlWriter.Create( memoryStream,
                                           new XmlWriterSettings()
                                           {
                                               Indent = true, NamespaceHandling = NamespaceHandling.OmitDuplicates
                                           } );

            serializer.Serialize( writer, docObject );

            memoryStream.Seek( 0, SeekOrigin.Begin );
            await OutputMemoryStream( memoryStream );
        }
        catch( Exception ex )
        {
            Logger?.LogError( "XDocument not written to file '{file}', message was {mesg}", FilePath, ex.Message );
            return false;
        }

        return true;
    }

    protected virtual void InitializeColorPicker() => RouteColorPicker ??= (_, _) => GeoConstants.DefaultRouteColor;
    protected virtual void InitializeWidthPicker() => RouteWidthPicker ??= GeoExtensions.RouteWidthPicker;

    protected abstract TDoc GetRootObject( List<IImportedRoute> routes );

    protected virtual async Task OutputMemoryStream( MemoryStream memoryStream ) =>
        await File.WriteAllBytesAsync( FilePath, memoryStream.ToArray() );
}