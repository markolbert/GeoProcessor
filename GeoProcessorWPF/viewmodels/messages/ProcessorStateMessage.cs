namespace J4JSoftware.GeoProcessor
{
    public sealed class ProcessorStateMessage
    {
        public ProcessorStateMessage( ProcessorState state )
        {
            ProcessorState = state;
        }

        public ProcessorState ProcessorState { get; }
    }
}