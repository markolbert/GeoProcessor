using System.Xml.Serialization;

namespace J4JSoftware.GeoProcessor.Kml;

#pragma warning disable CS8618
public class ExtendedData
{
    [XmlElement("Data")]
    public DataElement[] DataElements { get; set; }
}
