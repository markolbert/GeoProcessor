using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;
using IDataProtector = Microsoft.AspNetCore.DataProtection.IDataProtector;

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

        [JsonPropertyName("Value")]
        private string ValueHidden
        {
            set => Value = value;
        }
    }
}