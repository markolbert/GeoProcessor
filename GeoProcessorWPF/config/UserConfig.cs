using System.Collections.Generic;

namespace J4JSoftware.GeoProcessor
{
    public class UserConfig : IUserConfig
    {
        public Dictionary<ProcessorType, APIKey> APIKeys { get; set; } = new();

        public string GetAPIKey( ProcessorType procType ) =>
            APIKeys.TryGetValue( procType, out var apiKey )
                ? apiKey.Value
                : string.Empty;
    }
}
