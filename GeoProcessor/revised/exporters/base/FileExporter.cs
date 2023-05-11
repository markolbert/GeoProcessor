using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.GeoProcessor;

public abstract class FileExporter<TDoc> : Exporter, IFileExporter
    where TDoc : class
{
    protected FileExporter(
        string fileType,
        ILoggerFactory? loggerFactory
    )
        : base( loggerFactory )
    {
        FileType = fileType;
    }

    public string FileType { get; }
    public string FilePath { get; set; } = string.Empty;
    public Color RouteColor { get; set; } = Color.Blue;

#pragma warning disable CS1998
    protected override async Task<bool> ExportInternalAsync( List<IImportedRoute> routes, CancellationToken ctx )
#pragma warning restore CS1998
    {
        if( string.IsNullOrEmpty( FilePath ) )
        {
            Logger?.LogError("Export file is undefined");
            return false;
        }

        var docObject = GetRootObject( routes );
        var serializer = new XmlSerializer( typeof( TDoc ) );
        
        try
        {
            var fs = new StreamWriter( FilePath );
            var writer = XmlWriter.Create( fs,
                                           new XmlWriterSettings()
                                           {
                                               Indent = true, NamespaceHandling = NamespaceHandling.OmitDuplicates
                                           } );

            serializer.Serialize( writer, docObject );
        }
        catch( Exception ex )
        {
            Logger?.LogError( "XDocument not written to file '{file}', message was {mesg}", FilePath, ex.Message );
            return false;
        }

        return true;
    }

    protected abstract TDoc GetRootObject( List<IImportedRoute> routes );
}