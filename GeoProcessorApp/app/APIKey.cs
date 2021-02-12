#region license

// Copyright 2021 Mark A. Olbert
// 
// This library or program 'GeoProcessorApp' is free software: you can redistribute it
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

namespace J4JSoftware.GeoProcessor
{
    public class APIKey
    {
        private string _encryptedKey = string.Empty;
        private string _key = string.Empty;

        public ProcessorType Type { get; set; }

        public string EncryptedValue
        {
            get => _encryptedKey;

            set
            {
                _encryptedKey = value;

                // no point decrypting empty or null strings
                if( string.IsNullOrEmpty( _encryptedKey ) )
                    return;

                if( CompositionRoot.Default.Unprotect( _encryptedKey, out var decrypted ) )
                    _key = decrypted!;
            }
        }

        [ JsonIgnore ]
        public string Value
        {
            get => _key;

            set
            {
                _key = value;

                // no point encrypting empty or null strings
                if( string.IsNullOrEmpty( _key ) )
                    return;

                if( CompositionRoot.Default.Protect( _key, out var encrypted ) )
                    _encryptedKey = encrypted!;
            }
        }

        [ JsonPropertyName( "Value" ) ]
        private string ValueHidden
        {
            set => Value = value;
        }
    }
}