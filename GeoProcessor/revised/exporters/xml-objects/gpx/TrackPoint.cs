using System;
using System.Xml.Serialization;

namespace J4JSoftware.GeoProcessor.Gpx;

public class TrackPoint
{
    [XmlAttribute("lat")]
    public double Latitude { get; set; }

    [XmlAttribute("lon")]
    public double Longitude { get; set; }

    [XmlElement("ele")]
    public double? Elevation { get; set; }
    public bool ShouldSerializeElevation()=> Elevation != null;

    [XmlElement("time")]
    public DateTime? Timestamp { get; set; }
    public bool ShouldSerializeTimestamp() => Timestamp != null;

    [XmlElement("desc", IsNullable = false)]
    public string? Description { get; set; }
    public bool ShouldSerializeDescription() => !string.IsNullOrEmpty( Description );
}
