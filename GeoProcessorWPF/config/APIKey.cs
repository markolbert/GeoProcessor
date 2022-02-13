#region license

// Copyright 2021 Mark A. Olbert
// 
// This library or program 'GeoProcessorWPF' is free software: you can redistribute it
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
using System.Text.Json.Serialization;
using J4JSoftware.DependencyInjection;

namespace J4JSoftware.GeoProcessor
{
    public class APIKey
    {
        private string _encryptedKey = string.Empty;
        private string _key = string.Empty;

        internal IJ4JProtection? Protection { get; private set; }

        public ProcessorType Type { get; set; }

        public string EncryptedValue
        {
            get
            {
                if( Protection == null )
                    return string.Empty;

                if( !string.IsNullOrEmpty( _encryptedKey ) )
                    return _encryptedKey;

                if( !Protection.Protect( _key!, out var encrypted ) )
                    return string.Empty;

                _encryptedKey = encrypted!;

                return _encryptedKey;
            }

            set
            {
                _encryptedKey = value;

                // force decryption if the cryption system is initialized
                if( Protection != null )
                    _key = string.Empty;
            }
        }

        [ JsonIgnore ]
        public string Value
        {
            get
            {
                if (Protection == null)
                    return string.Empty;

                if (!string.IsNullOrEmpty(_key))
                    return _key;

                if ( !Protection.Unprotect( _encryptedKey!, out var decrypted ) )
                    return string.Empty;

                _key = decrypted!;

                return _key;
            }

            set
            {
                _key = value;

                // force encryption if the cryption system is initialized
                if( Protection != null )
                    _encryptedKey = string.Empty;
            }
        }

        // this lets us hide Value from being written but still be able to read it 
        // (for example, from the user secrets store)
        [ JsonPropertyName( "Value" ) ]
        private string ValueHidden
        {
            set => Value = value;
        }

        public void Initialize( IJ4JProtection protection )
        {
            Protection = protection;
        }
    }
}