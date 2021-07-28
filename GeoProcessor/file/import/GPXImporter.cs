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
    [ Importer( ImportType.GPX ) ]
    public class GPXImporter : FileHandler, IImporter
    {
        private const string TrackName = "trk";
        private const string RouteName = "name";
        private const string TrackSegmentName = "trkSeg";
        private const string TrackPointName = "trkpt";
        private const string LongitudeName = "lon";
        private const string LatitudeName = "lat";

        public GPXImporter( IImportConfig config, J4JLogger? logger = null )
            : base( config, logger )
        {
            Type = GeoExtensions.GetTargetType<ImporterAttribute>( GetType() )!.Type;
        }

        public ImportType Type { get; }

        public async Task<List<PointSet>?> ImportAsync(
            string filePath,
            CancellationToken cancellationToken )
        {
            if( !File.Exists( filePath ) )
            {
                Logger?.Error<string>( "File '{0}' does not exist", filePath );
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
                Logger?.Error<string, string>( "Could not load file '{0}', exception was '{1}'", filePath, e.Message );
                return null;
            }

            var retVal = new List<PointSet>();
            var segNum = 0;

            foreach( var track in xDoc.Descendants()
                .Where( x => x.Name.LocalName.Equals( TrackName, StringComparison.OrdinalIgnoreCase ) ) )
            {
                var trkName =
                    track.Descendants().SingleOrDefault(
                        x => x.Name.LocalName.Equals( RouteName, StringComparison.OrdinalIgnoreCase ) )?.Value
                    ?? "Unnamed Route";

                foreach( var trackSeg in track.Descendants()
                    .Where( x => x.Name.LocalName.Equals( TrackSegmentName, StringComparison.OrdinalIgnoreCase ) ) )
                {
                    var curDoc = new PointSet
                    {
                        RouteName = segNum == 0 ? trkName : $"{trkName}-{segNum + 1}"
                    };

                    retVal.Add( curDoc );

                    LinkedListNode<Coordinate>? prevPoint = null;

                    foreach( var point in trackSeg.Descendants()
                        .Where( x =>
                            x.Name.LocalName.Equals( TrackPointName, StringComparison.OrdinalIgnoreCase ) ) )
                    {
                        if( !ValidateDouble( point, LongitudeName, "longitude", out var longitude )
                            || !ValidateDouble( point, LatitudeName, "latitude", out var latitude ) )
                            continue;

                        prevPoint = curDoc.Points.Count == 0
                            ? curDoc.Points.AddFirst( new Coordinate( latitude, longitude ) )
                            : curDoc.Points.AddAfter( prevPoint!, new Coordinate( latitude, longitude ) );

                        if( !cancellationToken.IsCancellationRequested )
                            continue;

                        Logger?.Information( "File load cancelled" );
                        return null;
                    }

                    segNum++;
                }
            }

            return retVal;
        }

        private bool ValidateDouble( XElement point, string attrName, string name, out double result )
        {
            result = 99999;

            var text = point.Attribute( attrName )?.Value;

            if( string.IsNullOrEmpty( text ) )
            {
                Logger?.Error( "Missing longitude value" );
                return false;
            }

            if( !double.TryParse( text!, out var retVal ) )
            {
                Logger?.Error<string, string>( "Unparseable {0} value '{1}'", name, text );
                return false;
            }

            result = retVal;

            return true;
        }
    }
}