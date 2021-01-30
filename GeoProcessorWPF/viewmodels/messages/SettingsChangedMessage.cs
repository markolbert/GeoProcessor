namespace J4JSoftware.GeoProcessor
{
    public sealed class SettingsChangedMessage
    {
        public SettingsChangedMessage( SettingsPage source )
        {
            Source = source;
        }

        public SettingsPage Source { get; }
    }
}