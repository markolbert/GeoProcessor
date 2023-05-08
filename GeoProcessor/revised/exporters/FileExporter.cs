using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.GeoProcessor;

public abstract class FileExporter : Exporter, IFileExporter
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

    public override async Task<bool> ExportAsync( IEnumerable<ExportedRoute> routes, CancellationToken ctx = default )
    {
        var xDoc = new XDocument( GetXDeclaration() );
        xDoc.Add( GetXmlRoot() );
        
        foreach( var route in routes )
        {
            xDoc.Root!.Add(GetRouteElement(route));
        }

        try
        {
            var fs = new StreamWriter( FilePath );
            var writer = new XmlTextWriter( fs );

            await xDoc.WriteToAsync( writer, ctx );
            await writer.FlushAsync();
            writer.Close();
        }
        catch( Exception ex )
        {
            Logger?.LogError( "XDocument not written to file '{file}', message was {mesg}", FilePath, ex.Message );
            return false;
        }

        return true;
    }

    protected abstract XDeclaration GetXDeclaration();
    protected abstract XElement GetXmlRoot();
    protected abstract XElement GetRouteElement( ExportedRoute route );
}