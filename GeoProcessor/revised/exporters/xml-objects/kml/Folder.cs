using System.Xml.Serialization;

namespace J4JSoftware.GeoProcessor.Kml;

#pragma warning disable CS8618
public class Folder
{
    [XmlElement("name", IsNullable = true)]
    public string? Name { get; set; }

    [XmlElement("Placemark")]
    public Placemark[] Placemarks { get; set; }
}
