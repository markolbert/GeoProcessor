using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace J4JSoftware.GeoProcessor.Kml;

#pragma warning disable CS8618
public class LineString
{
    [XmlElement("tessellate")]
    public bool Tessellate { get; set; }

    [ XmlElement( "coordinates" ) ]
    public string CoordinatesText { get; set; }
}
