#region copyright
// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// Distance.static.cs
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
using Microsoft.Extensions.Logging;

namespace J4JSoftware.GeoProcessor;

public partial class Distance
{
    public static bool TryParse( string text, out Distance? result, ILogger? logger = null )
    {
        result = null;

        var parts = text.Split( " ", StringSplitOptions.RemoveEmptyEntries );

        if( parts.Length != 2 )
        {
            logger?.LogError( "Found {tokens} tokens when parsing '{text}' instead of 2", parts.Length, text );
            return false;
        }

        if( !double.TryParse( parts[ 0 ], out var distValue ) )
        {
            logger?.LogError( "Could not parse '{value}' as a double", parts[ 0 ] );
            return false;
        }

        if( !Enum.TryParse( typeof(UnitTypes), parts[ 1 ], true, out var unitType ) )
        {
            logger?.LogError( "Could not parse '{value}' as a {type}", parts[ 1 ], typeof(UnitTypes) );
            return false;
        }

        result = new Distance( (UnitTypes) unitType, distValue );

        return true;
    }
}