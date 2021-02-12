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

using System.Collections.Generic;
using System.Drawing;

namespace J4JSoftware.GeoProcessor
{
    public class CachedAppConfig
    {
        public CachedAppConfig( IAppConfig src )
        {
            InputPath = src.InputFile.FilePath;
            OutputPath = src.OutputFile.FilePath;
            ProcessorType = src.ProcessorType;

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

        public string InputPath { get; }
        public string OutputPath { get; }
        public ProcessorType ProcessorType { get; }
        public Dictionary<ProcessorType, ProcessorInfo> Processors { get; } = new();
        public string APIKey { get; }
        public int RouteWidth { get; }
        public Color RouteColor { get; }
        public Color RouteHighlightColor { get; }
    }
}