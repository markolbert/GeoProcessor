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

        public Coordinate( string[] latLong )
        {
            Longitude = Convert.ToDouble(latLong[1]);
            Latitude = Convert.ToDouble( latLong[ 0 ] );
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
            return new BingMapsRESTToolkit.Coordinate
            {
                Latitude = Latitude,
                Longitude = Longitude
            };
        }
    }
}