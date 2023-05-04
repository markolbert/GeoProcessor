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

using System.Text.Json.Serialization;

namespace J4JSoftware.GeoProcessor;

public class ProcessorInfo
{
    private int _maxDistMult = 3;
    private Distance? _maxSep;

    public int MaxDistanceMultiplier
    {
        get => _maxDistMult;
        set => _maxDistMult = value <= 0 ? 3 : value;
    }

    [ JsonIgnore ]
    public Distance MaxSeparation
    {
        get
        {
            if( !Equals( _maxSep, null ) )
                return _maxSep ?? new Distance( UnitTypes.ft, 100 );

            if( Distance.TryParse( MaxSeparationText, out var temp ) )
                _maxSep = temp;

            return _maxSep ?? new Distance( UnitTypes.ft, 100 );
        }

        set
        {
            _maxSep = value;
            MaxSeparationText = _maxSep.ToString();
        }
    }

    [ JsonPropertyName( "MaxSeparation" ) ]
    private string MaxSeparationText { get; set; } = "100 ft";
}