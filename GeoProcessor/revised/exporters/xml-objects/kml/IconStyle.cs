using System.Xml.Serialization;

namespace J4JSoftware.GeoProcessor.Kml;

#pragma warning disable CS8618
public class IconStyle
{
    [XmlElement("color")]
    public string Color { get; set; }

    [XmlElement("colorMode")]
    public string ColorMode { get; set; }

    [XmlElement("Icon")]
    public IconSource Icon { get; set; }
}


