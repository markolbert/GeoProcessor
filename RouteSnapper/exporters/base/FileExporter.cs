#region copyright
// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// FileExporter.cs
//
// This file is part of JumpForJoy Software's GeoProcessor.
// 
// GeoProcessor is free software: you can redistribute it and/or modify it 
// under the terms of the GNU General Public License as published by the 
// Free Software Foundation, either version 3 of the License, or 
// (at your option) any later version.
// 
// GeoProcessor is distributed in the hope that it will be useful, but 
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
// or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License 
// for more details.
// 
// You should have received a copy of the GNU General Public License along 
// with GeoProcessor. If not, see <https://www.gnu.org/licenses/>.
#endregion

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.RouteSnapper;

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
        set => _filePath = GeoExtensions.ChangeFileExtension( value, FileType );
    }

    public Func<SnappedRoute, int, Color>? RouteColorPicker { get; set; }
    public Func<SnappedRoute, int, int>? RouteWidthPicker { get; set; }

    public override async Task<bool> ExportAsync( List<SnappedRoute> routes, CancellationToken ctx = default )
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

    protected abstract TDoc GetRootObject( List<SnappedRoute> routes );

    protected virtual async Task OutputMemoryStream( MemoryStream memoryStream ) =>
        await File.WriteAllBytesAsync( FilePath, memoryStream.ToArray() );
}