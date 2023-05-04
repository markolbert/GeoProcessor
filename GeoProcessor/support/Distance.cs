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
using System.ComponentModel;

namespace J4JSoftware.GeoProcessor;

public partial class Distance
{
    public Distance( UnitTypes unit, double originalValue )
    {
        Unit = unit;
        OriginalValue = originalValue;
    }

    public UnitTypes Unit { get; private set; }

    public double OriginalValue { get; private set; }

    public void ChangeUnitType( UnitTypes unitType )
    {
        Unit = unitType;
    }

    public void ChangeOriginalValue( double value )
    {
        OriginalValue = value;
    }

    public override string ToString()
    {
        return $"{OriginalValue} {Unit}";
    }

    public double GetValue( UnitTypes outUnit )
    {
        if( outUnit == Unit )
            return OriginalValue;

        return Unit switch
        {
            UnitTypes.ft => outUnit switch
            {
                UnitTypes.mi => OriginalValue / 5280,
                UnitTypes.m => OriginalValue * 0.3048,
                UnitTypes.km => OriginalValue * 0.0003048,
                _ => throw new InvalidEnumArgumentException( $"Unsupported unit type '{Unit}'" )
            },
            UnitTypes.km => outUnit switch
            {
                UnitTypes.mi => OriginalValue * 0.62137119223733,
                UnitTypes.m => OriginalValue * 1000,
                UnitTypes.ft => OriginalValue * 3280.8398950131,
                _ => throw new InvalidEnumArgumentException( $"Unsupported unit type '{Unit}'" )
            },
            UnitTypes.m => outUnit switch
            {
                UnitTypes.mi => OriginalValue * 0.00062137119223733,
                UnitTypes.km => OriginalValue / 1000,
                UnitTypes.ft => OriginalValue * 3.2808398950131,
                _ => throw new InvalidEnumArgumentException( $"Unsupported unit type '{Unit}'" )
            },
            UnitTypes.mi => outUnit switch
            {
                UnitTypes.m => OriginalValue * 1609.344,
                UnitTypes.km => OriginalValue * 1.609344,
                UnitTypes.ft => OriginalValue * 5280,
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
        return new( x.Unit, x.OriginalValue * multiplier );
    }

    public static Distance operator *( double multiplier, Distance x )
    {
        return new( x.Unit, x.OriginalValue * multiplier );
    }

    public static Distance operator /( Distance x, double divisor )
    {
        return new( x.Unit, x.OriginalValue / divisor );
    }

    public static Distance operator +( Distance a, Distance b )
    {
        return new( a.Unit, a.OriginalValue + b.GetValue( a.Unit ) );
    }

    public static Distance operator -( Distance a, Distance b )
    {
        return new( a.Unit, a.OriginalValue - b.GetValue( a.Unit ) );
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