using System.Collections.Generic;

namespace J4JSoftware.GeoProcessor
{
    public interface IUserConfig
    {
        Dictionary<ProcessorType, APIKey> APIKeys { get; set; }
        string GetAPIKey( ProcessorType procType );
    }
}