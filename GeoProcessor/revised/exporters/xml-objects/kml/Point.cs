using System.Xml.Serialization;

namespace J4JSoftware.GeoProcessor.Kml;

#pragma warning disable CS8618
public class Point
{
    [XmlElement("extrude")]
    public bool Extrude { get; set; }

    [XmlElement("altitudeMode")]
    public string AltitudeMode { get; set; } = "absolute";

    [XmlElement("coordinates")]
    public string CoordinatesText { get; set; }
}
