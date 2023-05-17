using System.Xml.Serialization;

namespace J4JSoftware.RouteSnapper.Kml;

#pragma warning disable CS8618
public class DataElement
{
    [XmlAttribute("name")]
    public string Name { get; set; }

    [XmlElement("value")]
    public string Value { get; set; }
}
