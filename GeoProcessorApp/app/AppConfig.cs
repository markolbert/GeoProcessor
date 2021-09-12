#region license

// Copyright 2021 Mark A. Olbert
// 
// This library or program 'GeoProcessorApp' is free software: you can redistribute it
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

using System.Collections.Generic;
using System.Drawing;
using System.IO;
using J4JSoftware.Logging;

namespace J4JSoftware.GeoProcessor
{
    public class AppConfig : IImportConfig, IExportConfig
    {
        public bool StoreAPIKey { get; set; }
        public bool RunInteractive { get; set; }

        public InputFileInfo InputFile { get; } = new();

        public ExportType ExportType
        {
            get => OutputFile.Type;
            set => OutputFile.Type = value;
        }

        public Dictionary<ProcessorType, ProcessorInfo> Processors { get; set; } =
            new();

        public Dictionary<ProcessorType, APIKey> APIKeys { get; set; } = new();

        public string DefaultRouteName { get; set; } = "Unnamed Route";

        public OutputFileInfo OutputFile { get; } = new();
        public int RouteWidth { get; set; } = 4;
        public Color RouteColor { get; set; } = Color.Red;
        public Color RouteHighlightColor { get; set; } = Color.DarkTurquoise;

        public ProcessorType ProcessorType { get; set; } = ProcessorType.None;

        public ProcessorInfo ProcessorInfo => Processors.ContainsKey( ProcessorType )
            ? Processors[ ProcessorType ]
            : new ProcessorInfo();

        public string APIKey
        {
            get => APIKeys.ContainsKey( ProcessorType ) ? APIKeys[ ProcessorType ].Value : string.Empty;

            set
            {
                if( APIKeys.ContainsKey( ProcessorType ) )
                    APIKeys[ ProcessorType ].Value = value;
                else APIKeys.Add( ProcessorType, new APIKey { Value = value, Type = ProcessorType } );
            }
        }

        public bool IsValid( IJ4JLogger? logger )
        {
            // if we're storing an API key there's nothing to check
            if( StoreAPIKey )
                return true;

            var filePath = InputFile.GetPath();

            if( !File.Exists( filePath ) )
            {
                logger?.Error<string>( "Source file '{0}' is not accessible", filePath );
                return false;
            }

            if( Processors?.Count == 0 )
            {
                logger?.Error( "No processors are defined" );
                return false;
            }

            if( !Processors!.ContainsKey( ProcessorType ) )
            {
                logger?.Error( "No {0} processor is defined", ProcessorType );
                return false;
            }

            if( string.IsNullOrEmpty( APIKey ) )
            {
                logger?.Error( "API key for {0} is not defined", ProcessorType );
                return false;
            }

            return true;
        }
    }
}