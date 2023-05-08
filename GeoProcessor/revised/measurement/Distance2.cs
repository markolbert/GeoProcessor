using System;
using System.Collections.Generic;

namespace J4JSoftware.GeoProcessor;

public record Distance2( UnitType Units, double Value ) : IComparable<Distance2>, IComparable
{
    public Distance2 ChangeUnits( UnitType newUnits ) => new( newUnits, Value.Convert( Units, newUnits ) );

    #region IComparable interface

    public int CompareTo( Distance2? other )
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

        return obj is Distance2 other
            ? CompareTo( other )
            : throw new ArgumentException( $"Object must be of type {nameof( Distance2 )}" );
    }

    public static bool operator<( Distance2? left, Distance2? right ) => Comparer<Distance2>.Default.Compare( left, right ) < 0;

    public static bool operator>( Distance2? left, Distance2? right ) => Comparer<Distance2>.Default.Compare( left, right ) > 0;

    public static bool operator<=( Distance2? left, Distance2? right ) => Comparer<Distance2>.Default.Compare( left, right ) <= 0;

    public static bool operator>=( Distance2? left, Distance2? right ) => Comparer<Distance2>.Default.Compare( left, right ) >= 0;

    #endregion

    #region Mathematical operators

    public static Distance2 operator/( Distance2 left, Distance2 right )
    {
        var rightConverted = right.ChangeUnits( left.Units );

        return new Distance2( left.Units, left.Value / rightConverted.Value );
    }

    #endregion
}
