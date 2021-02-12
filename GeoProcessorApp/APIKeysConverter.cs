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

using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace J4JSoftware.GeoProcessor
{
    public class APIKeysConverter : JsonConverter<AppConfig>
    {
        public override AppConfig? Read( ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options )
        {
            throw new NotImplementedException( "This converter cannot be used for deserializing AppConfig objects" );
        }

        public override void Write( Utf8JsonWriter writer, AppConfig value, JsonSerializerOptions options )
        {
            if( value.APIKeys == null )
                return;

            // we only want to serialize the APIKeys property
            writer.WriteStartObject();

            writer.WriteStartObject( "APIKeys" );

            foreach( var kvp in value.APIKeys
                .Where( k => k.Key.RequiresAPIKey() ) )
            {
                writer.WriteStartObject( kvp.Key.ToString() );

                writer.WriteString( "EncryptedValue", kvp.Value.EncryptedValue );

                writer.WriteEndObject();
            }

            writer.WriteEndObject();

            writer.WriteEndObject();
        }
    }
}