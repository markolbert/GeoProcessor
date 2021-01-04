using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace J4JSoftware.KMLProcessor
{
    public static class KMLExtensions
    {
        public static int ToAbgr( Color color, byte transparency = 0xFF )
        {
            var bytes = new byte[4];
            bytes[ 0 ] = color.R;
            bytes[ 1 ] = color.G;
            bytes[ 2 ] = color.B;
            bytes[ 3 ] = transparency;

            return BitConverter.ToInt32( bytes );
        }

        public static string ToAbgrHex(Color color, byte transparency = 0xFF)
        {
            return ToAbgr( color, transparency ).ToString( "x" );
        }

        public static int FromAbgrHex( string text )
        {
            if( int.TryParse( text, NumberStyles.HexNumber,null, out var temp ) )
                return temp;

            return -1;
        }

        public static Color ToColor( int code )
        {
            var bytes = BitConverter.GetBytes( code );

            return bytes.Length != 4 
                ? Color.White 
                : Color.FromArgb( bytes[ 3 ], bytes[ 0 ], bytes[ 1 ], bytes[ 2 ] );
        }

        public static Color ToColor(string text)
        {
            var bytes = BitConverter.GetBytes( FromAbgrHex( text ) );

            return bytes.Length != 4
                ? Color.White
                : Color.FromArgb(bytes[3], bytes[0], bytes[1], bytes[2]);
        }

        public static Coordinate TargetCoordinate = new Coordinate( 38.49203, -122.65806 );

        public static bool NearTargetPoint( this Coordinate point )
        {
            if( Math.Abs( point.Latitude - TargetCoordinate.Latitude ) < .00001
                && Math.Abs( point.Longitude - TargetCoordinate.Longitude ) < .00001 )
                return true;

            return false;

            //if (GetDistance(TargetCoordinate, point).GetValue(UnitTypes.mi) < 0.1)
            //    System.Diagnostics.Debugger.Break();
        }

        public static Distance GetDistance( Coordinate c1, Coordinate c2 )
        {
            var deltaLat = c2.LatitudeRadians - c1.LatitudeRadians;
            var deltaLong = c2.LongitudeRadians - c1.LongitudeRadians;

            var h1 = Math.Sin( deltaLat / 2 ) * Math.Sin( deltaLat / 2 ) +
                     Math.Cos( c1.LatitudeRadians ) * Math.Cos( c2.LatitudeRadians ) *
                     Math.Sin( deltaLong / 2 ) * Math.Sin( deltaLong / 2 );

            var h2 = 2 * Math.Asin( Math.Min( 1, Math.Sqrt( h1 ) ) );

            return new Distance( UnitTypes.mi, h2 * 3958.8 );
        }

        public static double GetBearing( Coordinate c1, Coordinate c2 )
        {
            var deltaLongitude = c2.LongitudeRadians - c1.LongitudeRadians;

            var y = Math.Sin( deltaLongitude ) * Math.Cos( c2.LatitudeRadians );

            var x = Math.Cos( c1.LatitudeRadians ) * Math.Sin( c2.LatitudeRadians )
                    - Math.Sin( c1.LatitudeRadians ) * Math.Cos( c2.LatitudeRadians ) * Math.Cos( deltaLongitude );

            var theta = Math.Atan2( y, x );

            return ( theta.ToDegrees() + 360 ) % 360;
        }

        public static (double avg, double stdev) GetBearingStatistics(
            this LinkedListNode<Coordinate> startNode,
            LinkedListNode<Coordinate> endNode )
        {
            var curNode = startNode;

            var bearings = new List<double>();

            while( curNode != endNode )
            {
                bearings.Add( GetBearing( curNode!.Value, curNode.Next!.Value ) );

                curNode = curNode!.Next;
            }

            return ( bearings.Average(), GetStandardDeviation( bearings ) );
        }

        private static double GetStandardDeviation( List<double> values )
        {
            if( values.Count == 0 )
                return 0.0;

            var avg = values.Average();
            var sum = values.Sum( d => ( d - avg ) * ( d - avg ) );

            return Math.Sqrt( sum / values.Count );
        }

        public static double ToRadians( this double degrees )
        {
            return degrees * Math.PI / 180;
        }

        public static double ToDegrees( this double radians )
        {
            return 180 * radians / Math.PI;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Interoperability",
            "CA1416:Validate platform compatibility",
            Justification = "<Pending>")]
        public static bool Encrypt(string data, out string? result)
        {
            result = null;

            var byteData = Encoding.UTF8.GetBytes(data);

            try
            {
                var encrypted = ProtectedData.Protect(byteData, null, scope: DataProtectionScope.CurrentUser);
                result = Encoding.UTF8.GetString(encrypted);

                return true;
            }
            catch
            {
                return false;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Interoperability",
            "CA1416:Validate platform compatibility",
            Justification = "<Pending>")]
        public static bool Decrypt(string data, out string? result)
        {
            result = null;

            var byteData = Encoding.UTF8.GetBytes(data);

            try
            {
                var decrypted = ProtectedData.Unprotect(byteData, null, scope: DataProtectionScope.CurrentUser);
                result = Encoding.UTF8.GetString(decrypted);

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}