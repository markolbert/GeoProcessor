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
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using J4JSoftware.Logging;

namespace J4JSoftware.GeoProcessor
{
    [ Exporter( ExportType.KMZ ) ]
    public class KMZExporter : KMLExporter
    {
        public KMZExporter( IExportConfig config, J4JLogger? logger = null )
            : base( config, logger )
        {
            Type = GeoExtensions.GetTargetType<ExporterAttribute>( GetType() )!.Type;
        }

        public override async Task<bool> ExportAsync( PointSet pointSet, int docIndex,
            CancellationToken cancellationToken )
        {
            var xDoc = CreateXDocument( pointSet );

            if( xDoc == null )
                return false;

            var exportConfig = (IExportConfig) Configuration;

            var curFilePath = string.Empty;

            try
            {
                curFilePath = exportConfig.OutputFile.GetPath( docIndex );

                await using var fileStream = File.Create( curFilePath );
                using var archive = new ZipArchive( fileStream, ZipArchiveMode.Create, true );

                var kmlEntry = archive.CreateEntry( $"{exportConfig.OutputFile.FileNameWithoutExtension}.kml" );
                await using var kmlStream = kmlEntry.Open();

                await xDoc!.SaveAsync( kmlStream, SaveOptions.None, cancellationToken );

                await fileStream.FlushAsync( cancellationToken );

                Logger?.Information<string>( "Wrote file '{0}'", curFilePath );
            }
            catch( Exception e )
            {
                Logger?.Information<string, string>(
                    "Export to file '{0}' failed, message was '{1}'",
                    curFilePath,
                    e.Message );

                return false;
            }

            return true;
        }
    }
}