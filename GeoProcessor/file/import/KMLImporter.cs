#region license

// Copyright 2021 Mark A. Olbert
// 
// This library or program 'GeoProcessor' is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public License as
// published by the Free Software Foundation, either version 3 of the License,
// or (at your option) any later version.
// 
// This library or program is distributed in the hope that it will be useful, but
// WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// General Public License for more details.
// 
// You should have received a copy of the GNU General Public License along with
// this library or program.  If not, see <https://www.gnu.org/licenses/>.

#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using J4JSoftware.Logging;

namespace J4JSoftware.GeoProcessor
{
    [ Importer( ImportType.KML ) ]
    public class KMLImporter : FileHandler, IImporter
    {
        public KMLImporter( 
            IGeoConfig config, 
            IJ4JLogger? logger = null 
            )
            : base( config, logger )
        {
            Type = GeoExtensions.GetTargetType<ImporterAttribute>( GetType() )!.Type;
        }

        public ImportType Type { get; protected set; }

        public virtual async Task<List<PointSet>?> ImportAsync( string filePath, CancellationToken cancellationToken )
        {
            if( !File.Exists( filePath ) )
            {
                Logger?.Error<string>( $"File '{0}' does not exist", filePath );
                return null;
            }

            XDocument? xDoc = null;

            try
            {
                using var readStream = File.OpenText( filePath );
                xDoc = await XDocument.LoadAsync( readStream, LoadOptions.None, cancellationToken );
            }
            catch( Exception e )
            {
                Logger?.Error<string, string>( $"Could not load file '{0}', exception was '{1}'", filePath, e.Message );
                return null;
            }

            return ProcessXDocumentAsync( xDoc );
        }

        protected List<PointSet>? ProcessXDocumentAsync( XDocument xDoc )
        {
            var coordElement = xDoc.Descendants()
                .SingleOrDefault( x => x.Name.LocalName == "coordinates" );

            if( coordElement == null )
            {
                Logger?.Error( "Could not find 'coordinates' element in XDocument" );
                return null;
            }

            // this next call changes the coordinate stream into a sequence of numbers
            // separated by spaces: longitude latitude 0 longitude latitude 0...
            // and then into an array of numbers
            var rawNumbers = coordElement.Value.Replace( "\t", " " )
                .Replace( "\n", " " )
                .Replace( ",", " " )
                .Split( ' ', StringSplitOptions.RemoveEmptyEntries )
                .ToList();

            var numPts = rawNumbers.Count / 3;
            if( numPts * 3 != rawNumbers.Count )
            {
                Logger?.Error( "Corrupt coordinate(s)" );
                return null;
            }

            var retVal = new PointSet
            {
                RouteName = xDoc.Root?.Descendants()
                                .FirstOrDefault( x =>
                                    x.Name.LocalName.Equals( "name", StringComparison.OrdinalIgnoreCase ) )?.Value
                            ?? "Unnamed Route"
            };

            LinkedListNode<Coordinate>? prevPoint = null;

            for( var ptNum = 0; ptNum < numPts; ptNum += 3 )
            {
                var curPoint = new Coordinate( rawNumbers[ ptNum + 1 ], rawNumbers[ ptNum ] );

                prevPoint = retVal.Points.Count == 0
                    ? retVal.Points.AddFirst( curPoint )
                    : retVal.Points.AddAfter( prevPoint!, curPoint );
            }

            return new List<PointSet> { retVal };
        }
    }
}