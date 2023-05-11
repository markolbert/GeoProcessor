using System.Collections.Generic;
using System.Xml.Serialization;
// ReSharper disable InconsistentNaming
#pragma warning disable CS8618

namespace J4JSoftware.GeoProcessor.Kml;

public class Document
{
    [XmlElement("name")]
    public string? Name { get; set; }
    private bool ShouldSerializeName() => !string.IsNullOrEmpty( Name );

    [ XmlElement( "Style" ) ]
    public StyleContainer[] Styles { get; set; }

    [XmlElement("Folder")]
    public Folder[] Folders { get; set; }
}