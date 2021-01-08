using System;

namespace J4JSoftware.GeoProcessor
{
    public class SecuredProcessorTypeAttribute : Attribute
    {
    }

    public enum ProcessorType
    {
        [SecuredProcessorType] 
        Bing,

        Distance,
        
        [SecuredProcessorType]
        Google,
        
        Undefined
    }
}
