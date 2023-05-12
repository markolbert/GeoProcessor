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

        var fileType = type.GetCustomAttribute<ImportFileTypeAttribute>();
        if( fileType == null || string.IsNullOrEmpty(fileType.FileType  ) )
        {
            Logger?.LogCritical( "{type} is not decorated with a valid {fileType}", type, typeof( ImportFileTypeAttribute ) );
            throw new ArgumentException( $"{type} is not decorated with a valid {typeof( ImportFileTypeAttribute )}" );
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
