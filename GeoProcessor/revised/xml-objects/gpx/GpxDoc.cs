using System.Xml.Serialization;
// ReSharper disable InconsistentNaming
#pragma warning disable CS8618

namespace J4JSoftware.GeoProcessor;

[XmlRoot(ElementName="gpx", DataType="http://www.w3.org/2001/XMLSchema",Namespace = "http://www.topografix.com/GPX/1/1")]
public class GpxDoc
{
    [XmlElement("trk")]
    public GpxTrack[] Tracks { get; set; }

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