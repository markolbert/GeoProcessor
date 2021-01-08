using System;

namespace J4JSoftware.GeoProcessor
{
    [ Flags ]
    public enum CoalesenceType
    {
        Distance = 1 << 0,
        Bearing = 1 << 1,

        All = Distance | Bearing,
        None = 0
    }
}