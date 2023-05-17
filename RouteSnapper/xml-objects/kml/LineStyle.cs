#region copyright
// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// LineStyle.cs
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

using System.Xml.Serialization;

namespace J4JSoftware.RouteSnapper.Kml;

#pragma warning disable CS8618
public class LineStyle
{
    [XmlElement("color")]
    public string Color { get; set; }

    [XmlElement("colorMode")]
    public string ColorMode { get; set; }

    [XmlElement("width")]
    public double Width { get; set; }

    [XmlElement("labelVisibility", Namespace = "http://www.google.com/kml/ext/2.2")]
    public bool LabelVisibility { get; set; }
}
