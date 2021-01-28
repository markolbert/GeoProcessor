namespace J4JSoftware.GeoProcessor
{
    public sealed class FileConfigurationMessage
    {
        public FileConfigurationMessage( bool isValid )
        {
            ConfigurationIsValid = isValid;
        }

        public bool ConfigurationIsValid { get; }
    }
}