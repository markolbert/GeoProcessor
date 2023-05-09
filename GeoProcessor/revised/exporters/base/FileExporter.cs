using System;
using System.Collections.Generic;
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

#pragma warning disable CS1998
    public override async Task<bool> ExportAsync( IEnumerable<ExportedRoute> routes, CancellationToken ctx = default )
#pragma warning restore CS1998
    {
        var docObject = GetDocumentObject( routes );
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

    protected abstract TDoc GetDocumentObject( IEnumerable<ExportedRoute> routes );
}