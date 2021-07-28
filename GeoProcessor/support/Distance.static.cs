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
using J4JSoftware.Logging;

namespace J4JSoftware.GeoProcessor
{
    public partial class Distance
    {
        public static bool TryParse( string text, out Distance? result, J4JLogger? logger = null )
        {
            result = null;

            var parts = text.Split( " ", StringSplitOptions.RemoveEmptyEntries );

            if( parts.Length != 2 )
            {
                logger?.Error<int, string>( "Found {0} tokens when parsing '{1}' instead of 2", parts.Length, text );
                return false;
            }

            if( !double.TryParse( parts[ 0 ], out var distValue ) )
            {
                logger?.Error<string>( "Could not parse '{0}' as a double", parts[ 0 ] );
                return false;
            }

            if( !Enum.TryParse( typeof(UnitTypes), parts[ 1 ], true, out var unitType ) )
            {
                logger?.Error( "Could not parse '{0}' as a {1}", parts[ 1 ], typeof(UnitTypes) );
                return false;
            }

            result = new Distance( (UnitTypes) unitType!, distValue );

            return true;
        }
    }
}