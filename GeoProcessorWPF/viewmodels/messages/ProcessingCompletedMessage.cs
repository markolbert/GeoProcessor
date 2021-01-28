namespace J4JSoftware.GeoProcessor
{
    public sealed class ProcessingCompletedMessage
    {
        public ProcessingCompletedMessage( bool succeeded )
        {
            Succeeded = succeeded;
        }

        public bool Succeeded { get; }
    }
}