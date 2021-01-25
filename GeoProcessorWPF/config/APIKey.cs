using System.Text.Json.Serialization;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace J4JSoftware.GeoProcessor
{
    public class APIKey : ObservableObject
    {
        private string _encryptedKey = string.Empty;
        private string _key = string.Empty;

        public ProcessorType Type { get; set; }

        public string EncryptedValue
        {
            get => _encryptedKey;

            set
            {
                SetProperty( ref _encryptedKey, value );

                // no point decrypting empty or null strings
                if( string.IsNullOrEmpty( _encryptedKey ) )
                    return;

                if( CompositionRoot.Default.Unprotect( _encryptedKey, out var decrypted ) )
                    SetProperty( ref _key, decrypted! );
            }
        }

        [ JsonIgnore ]
        public string Value
        {
            get => _key;

            set
            {
                SetProperty( ref _key, value );

                // no point encrypting empty or null strings
                if( string.IsNullOrEmpty( _key ) )
                    return;

                if( CompositionRoot.Default.Protect( _key, out var encrypted ) )
                    SetProperty( ref _encryptedKey, encrypted! );
            }
        }

        [JsonPropertyName("Value")]
        private string ValueHidden
        {
            set => Value = value;
        }
    }
}