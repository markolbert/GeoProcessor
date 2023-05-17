#region copyright
// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// Placemark.cs
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
public class Placemark
{
    [XmlElement("name")]
    public string? Name { get; set; }
    private bool ShouldSerializeName() => !string.IsNullOrEmpty(Name);

    [XmlElement("description")]
    public string? Description { get; set; }
    private bool ShouldSerializeDescription() => !string.IsNullOrEmpty( Description );

    [XmlElement("styleUrl")]
    public string StyleUrl { get; set; }

    [XmlElement("visibility")]
    public bool Visibility { get; set; }

    [XmlElement("LineString")]
    public LineString? LineString { get; set; }
    private bool ShouldSerializeLineString() => LineString != null;

    [XmlElement("Point")]
    public Point? Point { get; set; }
    private bool ShouldSerializePoint() => Point != null;

    [XmlElement("ExtendedData")]
    public ExtendedData? ExtendedData { get; set; }
    private bool ShouldSerializeExtendedData() => ExtendedData != null;
}