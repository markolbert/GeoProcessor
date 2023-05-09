using System.Xml.Serialization;

namespace J4JSoftware.GeoProcessor;

[XmlRoot(ElementName="gpx", DataType="http://www.w3.org/2001/XMLSchema",Namespace = "http://www.topografix.com/GPX/1/1")]
public class GpxDoc
{
    [XmlElement("trk")]
    public GpxTrack[] Tracks { get; set; }

    // thanx to Shweta Lodha for this!!
    // https://www.c-sharpcorner.com/article/generating-xml-root-node-having-colon-via-serialization2/
    [XmlAttribute(Form = System.Xml.Schema.XmlSchemaForm.Qualified,
                  Namespace = "http://www.w3.org/2001/XMLSchema-instance")]
    public string xsi { get; set; } = "http://www.w3.org/2001/XMLSchema-instance";

    [XmlAttribute( Form = System.Xml.Schema.XmlSchemaForm.Qualified,
                    Namespace = "http://www.w3.org/2001/XMLSchema-instance" ) ]
    public string schemaLocation { get; set; } =
        "http://www.topografix.com/GPX/1/1 http://www.topografix.com/GPX/1/1/gpx.xsd";

    [XmlAttribute("version")]
    public string Version { get; set; } = "1.1";
}