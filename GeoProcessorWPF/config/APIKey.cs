using System;
using System.Text.Json.Serialization;
using J4JSoftware.DependencyInjection;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace J4JSoftware.GeoProcessor
{
    public class APIKey
    {
        private string? _encryptedKey;
        private string? _key;

        public void Initialize( IJ4JProtection protection )
        {
            Protection = protection;
        }

        internal IJ4JProtection? Protection { get; private set; }

        public ProcessorType Type { get; set; }

        public string EncryptedValue
        {
            get
            {
                if( _encryptedKey != null )
                    return _encryptedKey;

                // abort if we haven't been initialized or the plain text key is undefined/not yet set
                if( Protection == null || string.IsNullOrEmpty(_key) ) 
                    return string.Empty;

                if( Protection == null )
                    throw new NullReferenceException( $"{nameof(APIKey)} is not initialized. {nameof(Initialize)}() must be called before use." );

                if( !Protection.Protect( _key!, out var encrypted ) ) 
                    return string.Empty;

                _encryptedKey = encrypted!;
                return _encryptedKey;
            }

            set => _encryptedKey = value;
        }

        [ JsonIgnore ]
        public string Value
        {
            get
            {
                if( _key != null )
                    return _key;

                // abort if we haven't been initialized or the encrypted key is undefined/not yet set
                if( Protection == null || string.IsNullOrEmpty( _encryptedKey ) ) 
                    return string.Empty;

                if( !Protection.Unprotect( _encryptedKey!, out var decrypted ) ) 
                    return string.Empty;

                _key = decrypted!;
                return _key;
            }

            set
            {
                _key = value;

                // force re-encryption
                _encryptedKey = null;
            }
        }

        // this lets us hide Value from being written but still be able to read it 
        // (for example, from the user secrets store)
        [JsonPropertyName("Value")]
        private string ValueHidden
        {
            set => Value = value;
        }
    }
}