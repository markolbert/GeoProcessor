#region copyright
// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// KmzExporter2.cs
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
