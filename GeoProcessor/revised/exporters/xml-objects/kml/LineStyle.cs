using System.Xml.Serialization;

namespace J4JSoftware.GeoProcessor.Kml;

#pragma warning disable CS8618
public class LineStyle
{
    [XmlElement("color")]
    public string Color { get; set; }

    [XmlElement("colorMode")]
    public string ColorMode { get; set; }

    [XmlElement("width")]
    public int Width { get; set; }

    [XmlElement("labelVisibility", Namespace = "http://www.google.com/kml/ext/2.2")]
    public bool LabelVisibility { get; set; }
}
