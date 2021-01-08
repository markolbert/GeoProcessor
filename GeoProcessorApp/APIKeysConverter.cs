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

            writer.WriteStartObject("APIKeys");

            foreach( var kvp in value.APIKeys
                .Where( k => k.Key.IsSecuredProcessor() ) )
            {
                writer.WriteStartObject(kvp.Key.ToString());

                writer.WriteString( "EncryptedValue", kvp.Value.EncryptedValue );

                writer.WriteEndObject();
            }

            writer.WriteEndObject();

            writer.WriteEndObject();
        }
    }
}
