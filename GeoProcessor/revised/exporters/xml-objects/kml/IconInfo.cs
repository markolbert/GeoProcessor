using System.Xml.Serialization;

namespace J4JSoftware.GeoProcessor.Kml;

#pragma warning disable CS8618
public class IconInfo : LineStyle
{
    [XmlElement("Icon")]
    public IconSource Icon { get; set; }
}
