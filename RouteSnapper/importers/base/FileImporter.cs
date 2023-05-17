#region copyright
// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// FileImporter.cs
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
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using J4JSoftware.RouteSnapper.RouteBuilder;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.RouteSnapper;

public abstract class FileImporter<TDoc> : Importer, IFileImporter
    where TDoc : class
{
    protected FileImporter(
        string? mesgPrefix = null,
        ILoggerFactory? loggerFactory = null
    )
    :base( mesgPrefix, loggerFactory )
    {
        var type = GetType();

        var fileType = type.GetCustomAttribute<ImportFileTypeAttribute>();
        if( fileType == null || string.IsNullOrEmpty(fileType.FileType  ) )
        {
            Logger?.LogCritical( "{type} is not decorated with a valid {fileType}", type, typeof( ImportFileTypeAttribute ) );
            throw new ArgumentException( $"{type} is not decorated with a valid {typeof( ImportFileTypeAttribute )}" );
        }

        FileType = fileType.FileType;
    }

    public string FileType { get; }

    protected override async Task<List<Route>?> ImportInternalAsync( DataToImportBase toImport, CancellationToken ctx )
    {
        if (toImport is not FileToImport fileToImport)
        {
            Logger?.LogError("Expected a {correct} but got a {incorrect} instead",
                             typeof(FileToImport),
                             toImport.GetType());

            return null;
        }

        TDoc? xmlDoc;

        try
        {
            var streamReader = await GetStreamReaderAsync( fileToImport.FilePath );
            if( streamReader == null )
                return null;

            var xmlReader = XmlReader.Create(streamReader);
            var serializer = new XmlSerializer( typeof( TDoc ) );
            xmlDoc = serializer.Deserialize( xmlReader ) as TDoc;
        }
        catch (Exception ex)
        {
            Logger?.LogError("Exception encountered deserializing '{file}', message was {mesg}",
                             fileToImport.FilePath,
                             ex.Message);
            return null;
        }

        if( xmlDoc != null )
            return ProcessXmlDoc( xmlDoc );

        Logger?.LogError( "Could not deserialize '{file}' to {xType}", fileToImport.FilePath, typeof( TDoc ) );
        return null;
    }

#pragma warning disable CS1998
    protected virtual async Task<StreamReader?> GetStreamReaderAsync( string filePath )
#pragma warning restore CS1998
    {
        try
        {
            return new StreamReader( filePath );
        }
        catch( Exception ex )
        {
            Logger?.LogError( "Could not open file '{file}' to import, message was '{mesg}'", filePath, ex.Message );
        }

        return null;
    }

    protected abstract List<Route>? ProcessXmlDoc( TDoc xmlDoc );
}
