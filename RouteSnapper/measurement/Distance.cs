#region copyright
// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// Distance.cs
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

using System;
using System.Collections.Generic;

namespace J4JSoftware.RouteSnapper;

public record Distance( UnitType Units, double Value ) : IComparable<Distance>, IComparable
{
    public static Distance Invalid { get; } = new(UnitType.Kilometers, double.NaN);
    public static Distance Zero { get; } = new(UnitType.Kilometers, 0);
    public static Distance Infinity { get; } = new(UnitType.Kilometers, double.PositiveInfinity);

    public Distance ChangeUnits( UnitType newUnits ) => new( newUnits, Value.Convert( Units, newUnits ) );

    #region IComparable interface

    public int CompareTo( Distance? other )
    {
        if( ReferenceEquals( this, other ) )
            return 0;
        if( ReferenceEquals( null, other ) )
            return 1;

        var selfMeters = this.Convert( UnitType.Meters );
        var otherMeters = other.Convert( UnitType.Meters );

        return selfMeters.CompareTo( otherMeters );
    }

    public int CompareTo( object? obj )
    {
        if( ReferenceEquals( null, obj ) )
            return 1;
        if( ReferenceEquals( this, obj ) )
            return 0;

        return obj is Distance other
            ? CompareTo( other )
            : throw new ArgumentException( $"Object must be of type {nameof( Distance )}" );
    }

    public static bool operator<( Distance? left, Distance? right ) => Comparer<Distance>.Default.Compare( left, right ) < 0;

    public static bool operator>( Distance? left, Distance? right ) => Comparer<Distance>.Default.Compare( left, right ) > 0;

    public static bool operator<=( Distance? left, Distance? right ) => Comparer<Distance>.Default.Compare( left, right ) <= 0;

    public static bool operator>=( Distance? left, Distance? right ) => Comparer<Distance>.Default.Compare( left, right ) >= 0;

    #endregion

    #region Mathematical operators

    public static Distance operator/( Distance left, Distance right )
    {
        var rightConverted = right.ChangeUnits( left.Units );

        return new Distance( left.Units, left.Value / rightConverted.Value );
    }

    #endregion
}
