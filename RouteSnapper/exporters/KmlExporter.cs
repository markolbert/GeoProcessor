#region copyright
// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// KmlExporter.cs
//
// This file is part of JumpForJoy Software's GeoProcessor.
// 
// GeoProcessor is free software: you can redistribute it and/or modify it 
// under the terms of the GNU General Public License as published by the 
// Free Software Foundation, either version 3 of the License, or 
// (at your option) any later version.
// 
// GeoProcessor is distributed in the hope that it will be useful, but 
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
// or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License 
// for more details.
// 
// You should have received a copy of the GNU General Public License along 
// with GeoProcessor. If not, see <https://www.gnu.org/licenses/>.
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using J4JSoftware.RouteSnapper.Kml;
using J4JSoftware.VisualUtilities;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.RouteSnapper;

public class KmlExporter : FileExporter<Root>
{
    private const string LineStyleName = "line-style";

    private readonly Dictionary<int, string> _styles = new();

    public KmlExporter(
        ILoggerFactory? loggerFactory
    )
        : base( "kml", loggerFactory )
    {
    }

    protected KmlExporter(
        string fileType,
        ILoggerFactory? loggerFactory
    )
        : base( fileType, loggerFactory )
    {
    }

    protected override void InitializeColorPicker() => RouteColorPicker = GeoExtensions.RouteColorPicker;

    protected override Root GetRootObject( List<SnappedRoute> routes )
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

    private LineString CreateLineString( SnappedRoute route ) =>
        new()
        {
            Tessellate = true,
            CoordinatesText = route.SnappedPoints
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
}
