using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices;

namespace J4JSoftware.KMLProcessor
{
    public class Coordinate
    {
        public Coordinate( string latLong )
        {
            var parts = latLong.Split( ',' );

            Latitude = Convert.ToDouble( parts[ 0 ] );
            Longitude = Convert.ToDouble( parts[ 1 ] );
        }

        public Coordinate( double latitude, double longitude )
        {
            Latitude = latitude;
            Longitude = longitude;
        }

        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double LatitudeRadians => Latitude.ToRadians();
        public double LongitudeRadians => Longitude.ToRadians();
    }
}