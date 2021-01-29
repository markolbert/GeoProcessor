using System.Collections.Generic;

namespace J4JSoftware.GeoProcessor
{
    public interface IUserConfig
    {
        UserConfig Copy();
        void RestoreFrom( UserConfig src );
        Dictionary<ProcessorType, APIKey> APIKeys { get; set; }
        string GetAPIKey( ProcessorType procType );
    }
}