namespace J4JSoftware.GeoProcessor
{
    public sealed class SettingsReloadedMessage
    {
        public SettingsReloadedMessage( IAppConfig appConfig, IUserConfig userConfig )
        {
            AppConfig = appConfig;
            UserConfig = userConfig;
        }

        public IAppConfig AppConfig { get; }
        public IUserConfig UserConfig { get; }
    }
}