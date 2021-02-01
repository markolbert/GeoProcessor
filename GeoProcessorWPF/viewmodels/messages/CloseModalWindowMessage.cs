namespace J4JSoftware.GeoProcessor
{
    public sealed class CloseModalWindowMessage
    {
        public CloseModalWindowMessage( DialogWindow window )
        {
            Window = window;
        }

        public DialogWindow Window { get; }
    }
}