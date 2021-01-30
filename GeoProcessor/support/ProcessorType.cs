namespace J4JSoftware.GeoProcessor
{
    public enum ProcessorType
    {
        [ProcessorTypeInfo(true, true, 100)] 
        Bing,

        [ProcessorTypeInfo(false, false)] 
        Distance,
        
        [ProcessorTypeInfo(true, true, 100)] 
        Google,
        
        [ProcessorTypeInfo(false, false)] 
        None
    }
}
