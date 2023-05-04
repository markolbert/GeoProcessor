#region copyright
// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// GeoConfig.cs
//
// This file is part of JumpForJoy Software's Test.GeoProcessor.
// 
// Test.GeoProcessor is free software: you can redistribute it and/or modify it 
// under the terms of the GNU General Public License as published by the 
// Free Software Foundation, either version 3 of the License, or 
// (at your option) any later version.
// 
// Test.GeoProcessor is distributed in the hope that it will be useful, but 
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
// or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License 
// for more details.
// 
// You should have received a copy of the GNU General Public License along 
// with Test.GeoProcessor. If not, see <https://www.gnu.org/licenses/>.
#endregion

using System.Collections.Generic;
using System.Drawing;
using J4JSoftware.GeoProcessor;
#pragma warning disable 8618

namespace Test.GeoProcessor;

public class GeoConfig : IExportConfig, IImportConfig
{
    public Dictionary<ProcessorType, APIKeyValue> APIKeys { get; set; }

    public ProcessorType ProcessorType { get; set; } = ProcessorType.Google;

    public ProcessorInfo ProcessorInfo { get; } = new()
    {
        MaxSeparation = new Distance( UnitTypes.km, 2 ),
        MaxDistanceMultiplier = 3
    };

    public OutputFileInfo OutputFile { get; } = new();
    public int RouteWidth { get; set; } = 4;
    public Color RouteColor { get; set; } = Color.Red;
    public Color RouteHighlightColor { get; set; } = Color.DarkTurquoise;

    public string APIKey
    {
        get => APIKeys.ContainsKey( ProcessorType ) ? APIKeys[ ProcessorType ].Value : string.Empty;

        set
        {
            if( APIKeys.TryGetValue( ProcessorType, out var apiKey ) )
                apiKey.Value = value;
        }
    }

    public class APIKeyValue
    {
        public string Value { get; set; }
    }
}