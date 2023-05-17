#region copyright
// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// KmlImporter.cs
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
using J4JSoftware.RouteSnapper.Kml;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.RouteSnapper;

[ ImportFileType( "kml" ) ]
public class KmlImporter : FileImporter<Root>
{
    private static char[] CoordinateSeparators { get; } = { '\n', ' ' };

    public KmlImporter(
        ILoggerFactory? loggerFactory = null
    )
        : base( null, loggerFactory )
    {
    }

    // Garmin includes a LOT of additional/redundant information in addition
    // to the simple track points we need. Those details can cause problems
    // processing a Garmin-generated file, so set this to true to ignore them.
    public bool IgnoreGarminDetails { get; set; }

    protected override List<Route> ProcessXmlDoc( Root xmlDoc )
    {
        var retVal = new List<Route>();

        foreach( var folder in xmlDoc.Document.Folders )
        {
            // this is bizarrely complicated...but it works, and nothing else
            // I tried did, so change only at your own risk!! :)
            if( IgnoreGarminDetails )
            {
                // assume we won't skip this folder, and attempt to disprove
                // that assertion
                var skip = false;

                foreach( var placemark in folder.Placemarks )
                {
                    // if a placemark has no extended data it's not a candidate
                    // for tagging a folder as "should be skipped". If it has extended
                    // data, we need to check the Text Data element's value
                    var textElement = placemark.ExtendedData?.DataElements
                                               .FirstOrDefault(
                                                    x => x.Name.Equals( "text", StringComparison.OrdinalIgnoreCase ) );

                    // if the Text Data element exists and it's not empty/null, we need to 
                    // skip this folder
                    if( textElement == null || string.IsNullOrEmpty( textElement.Value ) )
                        continue;

                    skip = true;
                    break;
                }

                if( skip )
                    continue;
            }

            var importedRoute = new Route() { RouteName = folder.Name };

            foreach( var placemark in folder.Placemarks.Where(x=>x.LineString != null  ) )
            {
                var coordinates = ParseCoordinatesBlock( folder.Name ?? "Unnamed route",
                                                         placemark.LineString?.CoordinatesText );

                if( coordinates == null )
                    continue;

                importedRoute.Points.AddRange( coordinates );
            }

            if( importedRoute.Points.Any() )
                retVal.Add( importedRoute );
        }

        return retVal;
    }

    private List<Point>? ParseCoordinatesBlock( string routeName, string? text )
    {
        if( string.IsNullOrEmpty( text ) )
            return null;

        // coordinate tuples are supposed to be separated by spaces (with no
        // spaces within the tuple), but, at least in the Windows environment,
        // in practice they're separated by newline characters. Even maps.google.com
        // generates files using newline separators
        var lines = text.Split( '\n' )
                        .Select( x => x.Trim() )
                        .Where( x => !string.IsNullOrEmpty( x ) )
                        .ToList();

        if( !lines.Any() )
        {
            Logger?.LogError( "Could not parse lines in coordinate text area using newline separators for route '{route}'",
                              routeName );
            return null;
        }

        var retVal = new List<Point>();

        for( var idx = 0; idx < lines.Count; idx++ )
        {
            var coordinates = ParseCoordinates( lines[ idx ], idx, routeName );
            if( coordinates != null )
                retVal.Add( coordinates );
        }

        return retVal;
    }

    private Point? ParseCoordinates( string line, int lineNum, string routeName )
    {
        var textParts = line.Split( ',' );

        double[] valueParts;

        try
        {
            valueParts = textParts.Select( double.Parse ).ToArray();
        }
        catch
        {
            Logger?.LogError( "Invalid coordinate values in line {lineNum} of route '{route}'", lineNum, routeName );
            return null;
        }

        if( valueParts.Length is 2 or 3 )

            // whatever idiot thought it would be a good idea to put geo coordinates in
            // longitude, latitude, elevation order should be strung up by his or her thumbs!!
            // NO ONE WORKS WITH GEO COORDINATES THAT WAY!!!!
            return valueParts.Length switch
            {
                2 => new Point { Latitude = valueParts[ 1 ], Longitude = valueParts[ 0 ] },
                3 => new Point
                {
                    Latitude = valueParts[ 1 ], Longitude = valueParts[ 0 ], Elevation = valueParts[ 2 ]
                },
                _ => null // shouldn't ever get here
            };

        Logger?.LogError( "Could not parse coordinate line {lineNum} for route '{route}'", lineNum, routeName );
        return null;
    }
}
