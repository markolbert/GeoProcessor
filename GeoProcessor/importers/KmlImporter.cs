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

using System.Collections.Generic;
using System.Linq;
using J4JSoftware.GeoProcessor.Kml;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.GeoProcessor;

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

    protected override List<ImportedRoute> ProcessXmlDoc( Root xmlDoc )
    {
        var retVal = new List<ImportedRoute>();

        foreach( var folder in xmlDoc.Document.Folders )
        {
            var importedRoute = new ImportedRoute( new List<Coordinates>() )
            {
                RouteName = folder.Name
            };

            foreach( var placemark in folder.Placemarks.Where(x=>x.LineString != null  ) )
            {
                var coordinates = ParseCoordinatesBlock( folder.Name ?? "Unnamed route",
                                                    placemark.LineString!.CoordinatesText );

                if( coordinates == null )
                    continue;

                importedRoute.Points.AddRange( coordinates );
            }

            if( importedRoute.Points.Any() )
                retVal.Add( importedRoute );
        }

        return retVal;
    }

    private List<Coordinates>? ParseCoordinatesBlock( string routeName, string text )
    {
        // coordinate tuples are supposed to be separated by spaces (with no
        // spaces within the tuple), but many programs appear to use a newline
        // character instead. Test for all the various kinds of separators.
        var maxLines = -1;
        var lineSeparator = ' ';

        foreach( var sepInfo in CoordinateSeparators.Select(
                    ( x) => new { NumLines = text.Split( x ).Length, LineSeparator = x } ) )
        {
            if( sepInfo.NumLines <= maxLines )
                continue;

            maxLines = sepInfo.NumLines;
            lineSeparator = sepInfo.LineSeparator;
        }

        var lines = text.Split( lineSeparator );
        if( !lines.Any() )
        {
            Logger?.LogError( "Could not parse lines in coordinate text area using '{lineSep} ' for route '{route}'",
                              lineSeparator,
                              routeName );
            return null;
        }

        var retVal = new List<Coordinates>();

        for( var idx = 0; idx < lines.Length; idx++ )
        {
            var coordinates = ParseCoordinates( lines[ idx ], idx, routeName );
            if( coordinates != null )
                retVal.Add( coordinates );
        }

        return retVal;
    }

    private Coordinates? ParseCoordinates( string line, int lineNum, string routeName )
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
                2 => new Coordinates( valueParts[ 1 ], valueParts[ 0 ] ),
                3 => new Coordinates( valueParts[ 1 ], valueParts[ 0 ] ) { Elevation = valueParts[ 2 ] },
                _ => null // shouldn't ever get here
            };

        Logger?.LogError( "Could not parse coordinate line {lineNum} for route '{route}'", lineNum, routeName );
        return null;
    }
}
