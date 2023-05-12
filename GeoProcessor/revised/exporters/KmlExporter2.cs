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
    private const string LineStyleName = "line-style";

    private readonly Dictionary<int, string> _styles = new();

    public KmlExporter2(
        ILoggerFactory? loggerFactory
    )
        : base( "kml", loggerFactory )
    {
    }

    protected KmlExporter2(
        string fileType,
        ILoggerFactory? loggerFactory
    )
        : base( fileType, loggerFactory )
    {
    }

    protected override void InitializeColorPicker() => RouteColorPicker = GeoExtensions.RouteColorPicker;

    protected override Root GetRootObject( List<IImportedRoute> routes )
    {
        var retVal = new Root()
        {
            Creator = "https://www.jumpforjoysoftware.com",
            Document = new Document
            {
                Name = $"GeoProcessor Export {DateTime.Now:G}"
            }
        };

        var styles = new List<StyleContainer>();
        var folders = new List<Folder>();

        // first set up the styles and endpoint icons, if required
        for( var idx = 0; idx < routes.Count; idx++ )
        {
            var route = routes[ idx ];

            var curLineStyleName = $"{LineStyleName}-{idx}";
            
            styles.Add( new StyleContainer
            {
                Id = curLineStyleName,
                LineStyle = new LineStyle
                {
                    Color = RouteColorPicker!( route, idx ).ToAbgrHex(),
                    ColorMode = "normal",
                    LabelVisibility = false,
                    Width = RouteWidthPicker!( route, idx )
                }
            } );

            _styles.Add( idx, curLineStyleName );
        }

        var placemarks = new List<Placemark>();

        for( var idx = 0; idx < routes.Count; idx++ )
        {
            var route = routes[ idx ];

            placemarks.Clear();
            var lineStyle = _styles[ idx ];

            placemarks.Add( new Placemark
            {
                Name = route.RouteName,
                Description = route.Description,
                StyleUrl = $"#{lineStyle}",
                LineString = CreateLineString( route ),
                Visibility = true
            } );

            var folder = new Folder { Name = route.RouteName, Placemarks = placemarks.ToArray() };
            folders.Add( folder );
        }

        retVal.Document.Styles = styles.ToArray();
        retVal.Document.Folders = folders.ToArray();

        return retVal;
    }

    private LineString CreateLineString( IImportedRoute route ) =>
        new()
        {
            Tessellate = true,
            CoordinatesText = route.Aggregate( new StringBuilder(),
                                               ( sb, c ) =>
                                               {
                                                   if( sb.Length > 0 )
                                                       sb.Append( Environment.NewLine );

                                                   sb.Append( $"{c.Longitude},{c.Latitude},{c.Elevation ?? 0d}" );

                                                   return sb;
                                               },
                                               sb => sb.ToString() )
        };
}
