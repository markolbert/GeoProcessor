using System.Xml.Serialization;

namespace J4JSoftware.GeoProcessor;

public class GpxTrack
{
    [XmlElement("name", IsNullable = true)]
    public string Name { get; set; }
    public bool ShouldSerializeName() => !string.IsNullOrEmpty( Name );

    [XmlElement("desc", IsNullable = true)]
    public string Description { get; set; }
    public bool ShouldSerializeDescription() => !string.IsNullOrEmpty( Description );

    [XmlArray("trkseg")]
    [XmlArrayItem("trkpt")]
    public GpxTrackPoint[] TrackPoints { get; set; }
}
