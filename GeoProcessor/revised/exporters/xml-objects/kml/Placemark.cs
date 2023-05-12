using System.Xml.Serialization;

namespace J4JSoftware.GeoProcessor.Kml;

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

    public Point? Point { get; set; }
    private bool ShouldSerializePoint() => Point != null;
}