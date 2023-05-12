using System.Xml.Serialization;

namespace J4JSoftware.GeoProcessor.Kml;

#pragma warning disable CS8618
public class IconSource
{
    [ XmlElement( "href" ) ]
    public string HRef { get; set; } = GeoConstants.DefaultIconSourceHref;
}
