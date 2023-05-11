using System.Xml.Serialization;

namespace J4JSoftware.GeoProcessor.Kml;

#pragma warning disable CS8618

public class StyleContainer
{
    [XmlAttribute("id")]
    public string Id { get; set; }

    public LineStyle LineStyle { get; set; }
}
