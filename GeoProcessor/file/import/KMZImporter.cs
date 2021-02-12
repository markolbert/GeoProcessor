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
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using J4JSoftware.Logging;

namespace J4JSoftware.GeoProcessor
{
    [ Importer( ImportType.KMZ ) ]
    public class KMZImporter : KMLImporter
    {
        public KMZImporter( IGeoConfig config, IJ4JLogger? logger = null )
            : base( config, logger )
        {
            Type = GeoExtensions.GetTargetType<ImporterAttribute>( GetType() )!.Type;
        }

        public override async Task<List<PointSet>?> ImportAsync( string filePath, CancellationToken cancellationToken )
        {
            if( !File.Exists( filePath ) )
            {
                Logger?.Error<string>( $"File '{0}' does not exist", filePath );
                return null;
            }

            XDocument? xDoc = null;

            try
            {
                using var zipArchive = ZipFile.OpenRead( filePath );

                var kmlEntry = zipArchive.Entries
                    .FirstOrDefault( x => x.FullName.EndsWith( ".kml", StringComparison.OrdinalIgnoreCase ) );

                if( kmlEntry == null )
                {
                    Logger?.Error<string>( $"File '{0}' does not contain any KML files", filePath );
                    return null;
                }

                xDoc = await XDocument.LoadAsync( kmlEntry.Open(), LoadOptions.None, cancellationToken );
            }
            catch( Exception e )
            {
                Logger?.Error<string, string>( $"Could not load file '{0}', exception was '{1}'", filePath, e.Message );
                return null;
            }

            return ProcessXDocumentAsync( xDoc );
        }
    }
}