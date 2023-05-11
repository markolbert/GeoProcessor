using System.Xml.Serialization;

namespace J4JSoftware.GeoProcessor.Kml;

#pragma warning disable CS8618
[XmlRoot(ElementName="kml", DataType="http://www.w3.org/2001/XMLSchema",Namespace = "http://www.opengis.net/kml/2.2")]
public class Root
{
    [XmlAttribute("creator")]
    public string Creator { get; set; }

    public Document Document { get; set; }
}
