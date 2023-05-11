using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using J4JSoftware.GeoProcessor.Kml;
using J4JSoftware.VisualUtilities;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.GeoProcessor;

public class KmlExporter2 : FileExporter<Root>
{
    private const string LineStyleName = "linestyle";

    public KmlExporter2(
        ILoggerFactory? loggerFactory
    )
        : base( "kml", loggerFactory )
    {
    }

    protected override Root GetRootObject( List<IImportedRoute> routes )
    {
        var retVal = new Root()
        {
            Creator = "https://www.jumpforjoysoftware.com",
            Document = new Document
            {
                Name = $"GeoProcessor Export {DateTime.Now:G}",
                Styles = new[]
                {
                    new StyleContainer
                    {
                        Id = LineStyleName,
                        LineStyle = new LineStyle
                        {
                            Color = RouteColor.ToAbgrHex(),
                            ColorMode = "normal",
                            LabelVisibility = false,
                            Width = 10
                        }
                    }
                }
            }
        };

        var folders = new List<Folder>();

        foreach( var route in routes )
        {
            var lineString = new LineString
            {
                Tessellate = true,
                CoordinatesText = route.ToList()
                                       .Aggregate( new StringBuilder(),
                                                   ( sb, c ) =>
                                                   {
                                                       if( sb.Length > 0 )
                                                           sb.Append( Environment.NewLine );

                                                       sb.Append( $"{c.Longitude},{c.Latitude},{c.Elevation ?? 0d}" );

                                                       return sb;
                                                   },
                                                   sb => sb.ToString() )
            };

            var placemark = new Placemark()
            {
                Name = route.RouteName,
                Description = route.Description,
                StyleUrl = $"#{LineStyleName}",
                LineString = lineString,
                Visibility = true
            };

            var folder = new Folder { Name = route.RouteName, Placemarks = new[] { placemark } };
            folders.Add( folder );
        }

        retVal.Document.Folders = folders.ToArray();

        return retVal;
    }
}
