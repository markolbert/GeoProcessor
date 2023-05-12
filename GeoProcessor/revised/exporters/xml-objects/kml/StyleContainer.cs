using System.Xml.Serialization;

namespace J4JSoftware.GeoProcessor.Kml;

#pragma warning disable CS8618

public class StyleContainer
{
    [ XmlAttribute( "id" ) ]
    public string Id { get; set; }

    public LineStyle? LineStyle { get; set; }
    private bool ShouldSerializeLineStyle() => LineStyle != null;
    public IconStyle? IconStyle { get; set; }
    private bool ShouldSerializeIconStyle() => IconStyle != null;
}
