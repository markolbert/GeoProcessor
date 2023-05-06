#region copyright
// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// GpxImporter.cs
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
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.GeoProcessor;

[ Importer( ImportType.GPX ) ]
public class GpxImporter : FileHandler, IImporter
{
    public GpxImporter( 
        IImportConfig config, 
        ILoggerFactory? loggerFactory = null 
    )
        : base( config, loggerFactory )
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
            Logger?.LogError( "File '{path}' does not exist", filePath );
            return null;
        }

        XDocument? xDoc;

        try
        {
            using var readStream = File.OpenText( filePath );
            xDoc = await XDocument.LoadAsync( readStream, LoadOptions.None, cancellationToken );
        }
        catch( Exception e )
        {
            Logger?.LogError( "Could not load file '{path}', exception was '{mesg}'", filePath, e.Message );
            return null;
        }

        var retVal = new List<PointSet>();
        var segNum = 0;

        foreach( var track in xDoc.Descendants()
                                  .Where( x => x.Name.LocalName.Equals( GeoConstants.TrackName, StringComparison.OrdinalIgnoreCase ) ) )
        {
            var trkName =
                track.Descendants().SingleOrDefault(
                    x => x.Name.LocalName.Equals( GeoConstants.RouteName, StringComparison.OrdinalIgnoreCase ) )?.Value
             ?? "Unnamed Route";

            foreach( var trackSeg in track.Descendants()
                                          .Where( x => x.Name.LocalName.Equals( GeoConstants.TrackSegmentName, StringComparison.OrdinalIgnoreCase ) ) )
            {
                var curDoc = new PointSet
                {
                    RouteName = segNum == 0 ? trkName : $"{trkName}-{segNum + 1}"
                };

                retVal.Add( curDoc );

                LinkedListNode<Coordinate>? prevPoint = null;

                foreach( var point in trackSeg.Descendants()
                                              .Where( x =>
                                                          x.Name.LocalName.Equals( GeoConstants.TrackPointName, StringComparison.OrdinalIgnoreCase ) ) )
                {
                    if( !ValidateDouble( point, GeoConstants.LongitudeName, "longitude", out var longitude )
                    || !ValidateDouble( point, GeoConstants.LatitudeName, "latitude", out var latitude ) )
                        continue;

                    prevPoint = curDoc.Points.Count == 0
                        ? curDoc.Points.AddFirst( new Coordinate( latitude, longitude ) )
                        : curDoc.Points.AddAfter( prevPoint!, new Coordinate( latitude, longitude ) );

                    if( !cancellationToken.IsCancellationRequested )
                        continue;

                    Logger?.LogInformation( "File load cancelled" );
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
            Logger?.LogError( "Missing longitude value" );
            return false;
        }

        if( !double.TryParse( text, out var retVal ) )
        {
            Logger?.LogError( "Unparseable {name} value '{text}'", name, text );
            return false;
        }

        result = retVal;

        return true;
    }
}