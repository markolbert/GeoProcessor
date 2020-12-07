using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace J4JSoftware.KMLProcessor
{
    public class Coordinate
    {
        public Coordinate()
        {
        }

        public Coordinate( string latLong)
        {
            var parts = latLong.Split( ',' );

            Latitude = Convert.ToDouble( parts[0] );
            Longitude = Convert.ToDouble( parts[1] );
        }

        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public double MinDelta( Coordinate prior )
        {
            var latDelta = Math.Abs( Latitude - prior.Latitude );
            var longDelta = Math.Abs( Longitude - prior.Longitude );

            return latDelta < longDelta ? latDelta : longDelta;
        }
    }
}
