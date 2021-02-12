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
using System.Text.Json.Serialization;
using BingMapsRESTToolkit;

namespace J4JSoftware.GeoProcessor
{
    public class Coordinate
    {
        public Coordinate()
        {
        }

        public Coordinate( SnappedPoint bingPt )
        {
            Latitude = bingPt.Coordinate.Latitude;
            Longitude = bingPt.Coordinate.Longitude;
            Label = bingPt.Name;
        }

        public Coordinate( string latitude, string longitude )
        {
            Latitude = Convert.ToDouble( latitude );
            Longitude = Convert.ToDouble( longitude );
        }

        public Coordinate( double latitude, double longitude )
        {
            Latitude = latitude;
            Longitude = longitude;
        }

        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string? Label { get; set; }

        [ JsonIgnore ]
        public double LatitudeRadians => Latitude.ToRadians();

        [ JsonIgnore ]
        public double LongitudeRadians => Longitude.ToRadians();

        public BingMapsRESTToolkit.Coordinate ToBingMapsCoordinate()
        {
            return new()
            {
                Latitude = Latitude,
                Longitude = Longitude
            };
        }
    }
}