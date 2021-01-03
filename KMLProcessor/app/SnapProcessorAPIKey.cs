namespace J4JSoftware.KMLProcessor
{
    public class SnapProcessorAPIKey
    {
        private string _encryptedKey = string.Empty;

        public SnapProcessorType Type { get; set; }

        public string EncryptedAPIKey
        {
            get => _encryptedKey;

            set
            {
                _encryptedKey = value;

                APIKey = KMLExtensions.Decrypt( _encryptedKey, out var decryptedKey ) 
                    ? decryptedKey! 
                    : string.Empty;
            }
        }

        public string APIKey { get; set; } = string.Empty;
    }
}