using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;

namespace J4JSoftware.KMLProcessor
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

                _key = Decrypt(_encryptedKey, out var decryptedKey)
                    ? decryptedKey!
                    : string.Empty;
            }
        }

        [ JsonIgnore ]
        public string Value
        {
            get => _key;

            set
            {
                _key = value;

                _encryptedKey = Encrypt( _key, out var encryptedKey )
                    ? encryptedKey!
                    : string.Empty;
            }
        }

        [JsonPropertyName("Value")]
        private string ValueHidden
        {
            set => Value = value;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Interoperability",
            "CA1416:Validate platform compatibility",
            Justification = "<Pending>")]
        private bool Encrypt(string data, out string? result)
        {
            result = null;

            var byteData = Encoding.UTF8.GetBytes(data);

            try
            {
                var encrypted = ProtectedData.Protect(byteData, null, scope: DataProtectionScope.CurrentUser);
                result = Encoding.UTF8.GetString(encrypted);

                return true;
            }
            catch
            {
                return false;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Interoperability",
            "CA1416:Validate platform compatibility",
            Justification = "<Pending>")]
        private bool Decrypt(string data, out string? result)
        {
            result = null;

            var byteData = Encoding.UTF8.GetBytes(data);

            try
            {
                var decrypted = ProtectedData.Unprotect(byteData, null, scope: DataProtectionScope.CurrentUser);
                result = Encoding.UTF8.GetString(decrypted);

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}