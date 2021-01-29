using System.Collections.Generic;

namespace J4JSoftware.GeoProcessor
{
    public class UserConfig : IUserConfig
    {
        public UserConfig()
        {
        }

        private UserConfig( UserConfig src )
        {
            foreach( var kvp in src.APIKeys )
            {
                var temp = new APIKey();

                temp.Initialize( kvp.Value.Protection! );
                temp.Value = kvp.Value.Value;

                APIKeys.Add( kvp.Key, temp );
            }
        }

        public UserConfig Copy()
        {
            return new UserConfig( this );
        }

        public void RestoreFrom( UserConfig src )
        {
            APIKeys.Clear();

            foreach( var kvp in src.APIKeys )
            {
                var temp = new APIKey();

                temp.Initialize( kvp.Value.Protection! );
                temp.Value = kvp.Value.Value;

                APIKeys.Add( kvp.Key, temp );
            }
        }

        public Dictionary<ProcessorType, APIKey> APIKeys { get; set; } = new();

        public string GetAPIKey( ProcessorType procType ) =>
            APIKeys.TryGetValue( procType, out var apiKey )
                ? apiKey.Value
                : string.Empty;
    }
}
