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
using System.Xml;
using System.Xml.Serialization;
using J4JSoftware.GeoProcessor.Gpx;
using J4JSoftware.GeoProcessor.RouteBuilder;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.GeoProcessor;

[ImportFileType("gpx")]
public class GpxImporter : FileImporter
{
    public GpxImporter(
        ILoggerFactory? loggerFactory = null
    )
        : base( null, loggerFactory )
    {
    }

#pragma warning disable CS1998
    protected override async Task<List<ImportedRoute>> ImportInternalAsync( DataToImportBase toImport, CancellationToken ctx )
#pragma warning restore CS1998
    {
        var retVal = new List<ImportedRoute>();

        if ( toImport is not FileToImport fileToImport )
        {
            Logger?.LogError( "Expected a {correct} but got a {incorrect} instead",
                              typeof( FileToImport ),
                              toImport.GetType() );

            return retVal;
        }

        Root? test;

        try
        {
            var fs = new StreamReader( fileToImport.FilePath );
            var reader = XmlReader.Create( fs );
            var serializer = new XmlSerializer( typeof( Root ) );
            test = serializer.Deserialize( reader ) as Root;
        }
        catch( Exception ex )
        {
            Logger?.LogError( "Exception encountered deserializing '{file}', message was {mesg}",
                              fileToImport.FilePath,
                              ex.Message );
            return retVal;
        }

        if ( test == null )
        {
            Logger?.LogError( "Could not deserialize '{file}'", fileToImport.FilePath );
            return retVal;
        }

        foreach( var track in test.Tracks )
        {
            var trkName = track.Name;
            var trkDesc = track.Description;

            var importedRoute = new ImportedRoute( new List<Coordinates>() )
            {
                RouteName = trkName, Description = trkDesc
            };

            foreach( var trackPoint in track.TrackPoints )
            {
                var coordinate = new Coordinates( trackPoint.Latitude, trackPoint.Longitude )
                {
                    Elevation = trackPoint.Elevation,
                    Timestamp = trackPoint.Timestamp,
                    Description = string.IsNullOrEmpty( trackPoint.Description ) ? null : trackPoint.Description
                };

                importedRoute.Points.Add( coordinate );
            }

            if (importedRoute.Points.Any())
                retVal.Add(importedRoute);
        }

        return retVal;
    }
}
