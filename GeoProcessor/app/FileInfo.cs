#region copyright
// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// FileInfo.cs
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

#pragma warning disable 8618

namespace J4JSoftware.GeoProcessor;

public abstract class FileInfo<T>
    where T : Enum
{
    public string FilePath
    {
        get => GetPath();

        set
        {
            DirectoryPath = Path.GetDirectoryName( value ) ?? string.Empty;
            FileNameWithoutExtension = Path.GetFileNameWithoutExtension( value );
            Type = GetTypeFromExtension( Path.GetExtension( value ) );
        }
    }

    public string DirectoryPath { get; protected set; }
    public string FileNameWithoutExtension { get; set; }
    public T Type { get; set; }
    public string FileExtension => GetExtensionFromType( Type );

    public string GetPath( int fileNum = 0 )
    {
        fileNum = fileNum < 0 ? 0 : fileNum;

        var parts = new List<string>();

        if( !string.IsNullOrEmpty( DirectoryPath ) )
            parts.Add( DirectoryPath );

        if( !string.IsNullOrEmpty( FileNameWithoutExtension ) )
            parts.Add( fileNum > 0
                           ? $"{FileNameWithoutExtension}-{fileNum}{FileExtension}"
                           : $"{FileNameWithoutExtension}{FileExtension}" );

        return parts.Count == 0 ? string.Empty : Path.Combine( parts.ToArray() );
    }

    protected abstract T GetTypeFromExtension( string? ext );
    protected abstract string GetExtensionFromType( T type );
}