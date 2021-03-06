﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace J4JSoftware.GeoProcessor
{
    public static class GeoExtensions
    {
        public static bool SnapsToRoute( this ProcessorType procType )
        {
            var memInfo = typeof(ProcessorType).GetField( procType.ToString() );
            if( memInfo == null )
                return false;

            return memInfo.GetCustomAttribute<ProcessorTypeInfoAttribute>()?.IsSnapToRoute ?? false;
        }

        public static bool RequiresAPIKey( this ProcessorType procType )
        {
            var memInfo = typeof(ProcessorType).GetField( procType.ToString() );
            if( memInfo == null )
                return false;

            return memInfo.GetCustomAttribute<ProcessorTypeInfoAttribute>()?.RequiresAPIKey ?? false;
        }

        public static int MaxPointsPerRequest( this ProcessorType procType )
        {
            var memInfo = typeof(ProcessorType).GetField( procType.ToString() );
            if( memInfo == null )
                return 100;

            return memInfo.GetCustomAttribute<ProcessorTypeInfoAttribute>()?.MaxPointsPerRequest ?? 100;
        }

        public static TAttr? GetTargetType<THandler, TAttr>()
            where TAttr : Attribute
            => GetTargetType<TAttr>( typeof(THandler) );

        public static TAttr? GetTargetType<TAttr>( Type handlerType )
            where TAttr : Attribute
            => handlerType.GetCustomAttribute<TAttr>();

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
    }
}