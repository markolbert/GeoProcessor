#region copyright
// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// GpxExporter.cs
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
using J4JSoftware.RouteSnapper.Gpx;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.RouteSnapper;

public class GpxExporter : FileExporter<Root>
{
    public GpxExporter( 
        ILoggerFactory? loggerFactory 
    )
        : base( "gpx", loggerFactory )
    {
    }

    protected override void InitializeColorPicker() => RouteColorPicker = GeoExtensions.RouteColorPicker;

    protected override Root GetRootObject(List<SnappedRoute> routes)
    {
        var retVal = new Root()
        {
            Creator = "https://www.jumpforjoysoftware.com",
            SchemaLocation = "http://www.topografix.com/GPX/1/1 http://www.topografix.com/GPX/1/1/gpx.xsd",
            Version = "1.1"
        };

        var tracks = new List<Track>();

        foreach( var route in routes )
        {
            var track = new Track { Description = route.Description, Name = route.RouteName };
            tracks.Add( track );

            track.TrackPoints = route.SnappedPoints
                                     .Select( x => new TrackPoint
                                      {
                                          Latitude = x.Latitude,
                                          Longitude = x.Longitude,
                                          Timestamp = x.Timestamp,
                                          Description = x.Description,
                                          Elevation = x.Elevation
                                      } )
                                     .ToArray();
        }

        retVal.Tracks = tracks.ToArray();

        return retVal;
    }
}
