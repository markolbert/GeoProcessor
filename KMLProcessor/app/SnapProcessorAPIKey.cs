namespace J4JSoftware.KMLProcessor
{
    public class SnapProcessorAPIKey
    {
        public SnapProcessorType Type { get; set; }
        public string EncryptedAPIKey { get; set; } = string.Empty;

        public string GetAPIKey()
        {
            if( string.IsNullOrEmpty( EncryptedAPIKey ) )
                return string.Empty;

            if( !KMLExtensions.Decrypt( EncryptedAPIKey, out var retVal ) )
                return string.Empty;

            return retVal!;
        }
    }
}