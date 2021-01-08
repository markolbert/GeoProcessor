using System;
using System.Collections.Generic;
using System.IO;

#pragma warning disable 8618

namespace J4JSoftware.GeoProcessor
{
    public abstract class FileInfo<T>
        where T:Enum
    {
        protected FileInfo()
        {
        }

        public string FilePath
        {
            get => GetPath();

            set
            {
                DirectoryPath = Path.GetDirectoryName(value) ?? string.Empty;
                FileName = Path.GetFileNameWithoutExtension(value);
                Type = GetTypeFromExtension( Path.GetExtension( value ) );
            }
        }

        public string GetPath( int fileNum = 0 )
        {
            fileNum = fileNum < 0 ? 0 : fileNum;

            var parts = new List<string>();

            if( !string.IsNullOrEmpty( DirectoryPath ) )
                parts.Add( DirectoryPath );

            if( !string.IsNullOrEmpty(FileName  ))
                parts.Add( fileNum > 0 ? $"{FileName}-{fileNum}{FileExtension}" : $"{FileName}{FileExtension}" );

            return parts.Count == 0 ? string.Empty : Path.Combine( parts.ToArray() );
        }

        public string DirectoryPath { get; protected set; }
        public string FileName { get; set; }
        public T Type { get; set; }
        public string FileExtension => GetExtensionFromType( Type );

        protected abstract T GetTypeFromExtension( string? ext );
        protected abstract string GetExtensionFromType( T type );
    }
}