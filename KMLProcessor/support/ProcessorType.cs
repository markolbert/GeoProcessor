using System;

namespace J4JSoftware.KMLProcessor
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
