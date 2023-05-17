#region copyright
// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// Root.cs
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
// ReSharper disable InconsistentNaming
#pragma warning disable CS8618

namespace J4JSoftware.RouteSnapper.Gpx;

[XmlRoot(ElementName="gpx", DataType="http://www.w3.org/2001/XMLSchema",Namespace = "http://www.topografix.com/GPX/1/1")]
public class Root
{
    [XmlElement("trk")]
    public Track[] Tracks { get; set; }

    // thanx to Shweta Lodha for this!!
    // https://www.c-sharpcorner.com/article/generating-xml-root-node-having-colon-via-serialization2/
    [XmlAttribute(AttributeName = "schemaLocation", 
                  Form = System.Xml.Schema.XmlSchemaForm.Qualified,
                  Namespace = "http://www.w3.org/2001/XMLSchema-instance" ) ]
    public string SchemaLocation { get; set; }

    [XmlAttribute("version")]
    public string Version { get; set; }

    [XmlAttribute("creator")]
    public string Creator { get; set; }
}