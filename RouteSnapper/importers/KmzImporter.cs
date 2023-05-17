#region copyright
// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// KmzImporter.cs
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
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.RouteSnapper;

[ ImportFileType( "kmz" ) ]
public class KmzImporter : KmlImporter
{
    public KmzImporter(
        ILoggerFactory? loggerFactory = null
    )
        : base( loggerFactory )
    {
    }

#pragma warning disable CS1998
    protected override async Task<StreamReader?> GetStreamReaderAsync( string filePath )
#pragma warning restore CS1998
    {
        var memoryStream = new MemoryStream();

        try
        {
            using var archive = ZipFile.OpenRead( filePath );

            // we want the first (and should be only) kml file
            var entry = archive.Entries
                                   .FirstOrDefault( x => Path.GetExtension( x.Name )
                                                             .Equals( ".kml", StringComparison.OrdinalIgnoreCase ) );

            if( entry != null )
            {
                await entry.Open().CopyToAsync( memoryStream );
                memoryStream.Seek( 0, SeekOrigin.Begin );

                return new StreamReader( memoryStream );
            }

            Logger?.LogError("Could not open file '{file}' for reading", filePath);
        }
        catch( Exception ex )
        {
            Logger?.LogError( "Could not create {type} from file '{file}', message was '{mesg}'",
                              typeof( StreamReader ),
                              filePath,
                              ex.Message );
        }

        return null;
    }
}
