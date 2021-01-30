using System;

namespace J4JSoftware.GeoProcessor
{
    public class ProcessorTypeInfoAttribute : Attribute
    {
        public ProcessorTypeInfoAttribute( bool requiresApiKey, bool isSnapToRoute, int maxPtsPerRequest = Int32.MaxValue )
        {
            RequiresAPIKey = requiresApiKey;
            IsSnapToRoute = isSnapToRoute;
            MaxPointsPerRequest = maxPtsPerRequest;
        }

        public bool RequiresAPIKey { get; }
        public bool IsSnapToRoute { get; }
        public int MaxPointsPerRequest { get; }
    }
}