#region license

// Copyright 2021 Mark A. Olbert
// 
// This library or program 'GeoProcessor' is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public License as
// published by the Free Software Foundation, either version 3 of the License,
// or (at your option) any later version.
// 
// This library or program is distributed in the hope that it will be useful, but
// WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// General Public License for more details.
// 
// You should have received a copy of the GNU General Public License along with
// this library or program.  If not, see <https://www.gnu.org/licenses/>.

#endregion

using System;

namespace J4JSoftware.GeoProcessor;

public class InputFileInfo : FileInfo<ImportType>
{
    protected override ImportType GetTypeFromExtension( string? ext )
    {
        if( ext?.Length > 0
        && Enum.TryParse( typeof(ImportType), ext[ 1.. ], true, out var parsed ) )
            return (ImportType) parsed!;

        return ImportType.Unknown;
    }

    protected override string GetExtensionFromType( ImportType type )
    {
        return type switch
        {
            ImportType.GPX => ".gpx",
            ImportType.KML => ".kml",
            ImportType.KMZ => ".kmz",
            _ => string.Empty
        };
    }
}