using System;
using System.Text.Json.Serialization;

namespace J4JSoftware.KMLProcessor
{
    public class Coordinate
    {
        public Coordinate()
        {
        }

        public Coordinate( BingMapsRESTToolkit.SnappedPoint bingPt )
        {
            Latitude = bingPt.Coordinate.Latitude;
            Longitude = bingPt.Coordinate.Longitude;
            Label = bingPt.Name;
        }

        public Coordinate( string latLong )
        {
            var parts = latLong.Split( ',' );

            Latitude = Convert.ToDouble( parts[ 1 ] );
            Longitude = Convert.ToDouble( parts[ 0 ] );
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

        public BingMapsRESTToolkit.Coordinate ToBingMapsCoordinate() =>
            new BingMapsRESTToolkit.Coordinate
            {
                Latitude = Latitude,
                Longitude = Longitude
            };
    }
}