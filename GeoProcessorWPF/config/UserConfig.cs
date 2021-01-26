using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace J4JSoftware.GeoProcessor
{
    public class UserConfig : IUserConfig
    {
        public Dictionary<ProcessorType, APIKey> APIKeys { get; set; } = new();

        public string? GetAPIKey( ProcessorType procType ) =>
            APIKeys.TryGetValue( procType, out var apiKey )
                ? apiKey.Value
                : null;
    }
}
