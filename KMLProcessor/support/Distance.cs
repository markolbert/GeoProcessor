using System;
using System.ComponentModel;

namespace J4JSoftware.KMLProcessor
{
    public class Distance
    {
        public Distance( UnitTypes unit, double originalValue )
        {
            Unit = unit;
            OriginalValue = originalValue;
        }

        public UnitTypes Unit { get; }
        public double OriginalValue { get; }

        public double GetValue( UnitTypes outUnit )
        {
            if( outUnit == Unit )
                return OriginalValue;

            return Unit switch
            {
                UnitTypes.Feet => outUnit switch
                {
                    UnitTypes.Miles => OriginalValue / 5280,
                    UnitTypes.Meters => OriginalValue * 0.3048,
                    UnitTypes.Kilometers => OriginalValue * 0.0003048,
                    _ => throw new InvalidEnumArgumentException( $"Unsupported unit type '{Unit}'" )
                },
                UnitTypes.Kilometers => outUnit switch
                {
                    UnitTypes.Miles => OriginalValue * 0.62137119223733,
                    UnitTypes.Meters => OriginalValue * 1000,
                    UnitTypes.Feet => OriginalValue * 3280.8398950131,
                    _ => throw new InvalidEnumArgumentException( $"Unsupported unit type '{Unit}'" )
                },
                UnitTypes.Meters => outUnit switch
                {
                    UnitTypes.Miles => OriginalValue * 0.00062137119223733,
                    UnitTypes.Kilometers => OriginalValue / 1000,
                    UnitTypes.Feet => OriginalValue * 3.2808398950131,
                    _ => throw new InvalidEnumArgumentException( $"Unsupported unit type '{Unit}'" )
                },
                UnitTypes.Miles => outUnit switch
                {
                    UnitTypes.Meters => OriginalValue * 1609.344,
                    UnitTypes.Kilometers => OriginalValue * 1.609344,
                    UnitTypes.Feet => OriginalValue * 5280,
                    _ => throw new InvalidEnumArgumentException( $"Unsupported unit type '{Unit}'" )
                },
                _ => throw new InvalidEnumArgumentException( $"Unsupported unit type '{Unit}'" )
            };
        }

        #region operator overrides

        public static bool operator >( Distance a, Distance b )
        {
            return a.OriginalValue > b.GetValue( a.Unit );
        }

        public static bool operator <( Distance a, Distance b )
        {
            return a.OriginalValue < b.GetValue( a.Unit );
        }

        public static bool operator ==( Distance a, Distance b )
        {
            return Math.Abs( a.OriginalValue - b.GetValue( a.Unit ) ) < 1E-5;
        }

        public static bool operator !=( Distance a, Distance b )
        {
            return !( a == b );
        }

        public static bool operator >=( Distance a, Distance b )
        {
            return a == b || a > b;
        }

        public static bool operator <=( Distance a, Distance b )
        {
            return a == b || a < b;
        }

        public static Distance operator *( Distance x, double multiplier )
        {
            return new Distance( x.Unit, x.OriginalValue * multiplier );
        }

        public static Distance operator *( double multiplier, Distance x )
        {
            return new Distance( x.Unit, x.OriginalValue * multiplier );
        }

        public static Distance operator /( Distance x, double divisor )
        {
            return new Distance( x.Unit, x.OriginalValue / divisor );
        }

        public static Distance operator +( Distance a, Distance b )
        {
            return new Distance( a.Unit, a.OriginalValue + b.GetValue( a.Unit ) );
        }

        public static Distance operator -( Distance a, Distance b )
        {
            return new Distance( a.Unit, a.OriginalValue - b.GetValue( a.Unit ) );
        }

        public override int GetHashCode()
        {
            return HashCode.Combine( Unit, OriginalValue );
        }

        public override bool Equals( object? obj )
        {
            return obj switch
            {
                null => false,
                Distance castObj => castObj == this,
                _ => false
            };
        }

        #endregion
    }
}