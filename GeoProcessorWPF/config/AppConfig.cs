#region license

// Copyright 2021 Mark A. Olbert
// 
// This library or program 'GeoProcessorWPF' is free software: you can redistribute it
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
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using J4JSoftware.Logging;

namespace J4JSoftware.GeoProcessor
{
    public class AppConfig : IAppConfig
    {
        public AppConfig()
        {
            OutputFile.FilePath = Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.Desktop ),
                "GeoProcessor.kml" );
        }

        public void RestoreFrom( CachedAppConfig src )
        {
            InputFile.FilePath = src.InputPath;
            OutputFile.FilePath = src.OutputPath;
            ProcessorType = src.ProcessorType;

            Processors.Clear();

            foreach( var kvp in src.Processors )
            {
                var cachedPI = new ProcessorInfo
                {
                    MaxDistanceMultiplier = kvp.Value.MaxDistanceMultiplier,
                    MaxSeparation = new Distance( kvp.Value.MaxSeparation.Unit, kvp.Value.MaxSeparation.OriginalValue )
                };

                Processors.Add( kvp.Key, cachedPI );
            }

            APIKey = src.APIKey;
            RouteWidth = src.RouteWidth;
            RouteColor = src.RouteColor;
            RouteHighlightColor = src.RouteHighlightColor;
        }

        public NetEventSink? NetEventSink { get; set; }

        public InputFileInfo InputFile { get; } = new();
        public OutputFileInfo OutputFile { get; } = new();

        public ProcessorType ProcessorType { get; set; } = ProcessorType.None;

        public Dictionary<ProcessorType, ProcessorInfo> Processors { get; set; } =
            new();

        public ProcessorInfo? ProcessorInfo => Processors.TryGetValue( ProcessorType, out var retVal ) ? retVal : null;

        public string APIKey { get; set; } = string.Empty;

        public ExportType ExportType
        {
            get => OutputFile.Type;
            set => OutputFile.Type = value;
        }

        public int RouteWidth { get; set; } = 4;
        public Color RouteColor { get; set; } = Color.Red;
        public Color RouteHighlightColor { get; set; } = Color.DarkTurquoise;
    }
}