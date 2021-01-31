using System.IO;
using System.Text.Json;
using System.Windows.Input;
using J4JSoftware.Logging;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
#pragma warning disable 8618

namespace J4JSoftware.GeoProcessor
{
    public class DesignTimeOptionsViewModel : ObservableRecipient, IOptionsViewModel
    {
        private readonly IAppConfig _appConfig;
        private readonly IUserConfig _userConfig;
        private readonly IJ4JLogger? _logger;

        private CachedAppConfig _cachedAppConfig;
        private UserConfig _prevUserConfig;

        private bool _settingsChanged;

        public DesignTimeOptionsViewModel( 
            IAppConfig appConfig,
            IUserConfig userConfig,
            IJ4JLogger? logger 
            )
        {
            _appConfig = appConfig;
            _userConfig = userConfig;

            _logger = logger;
            _logger?.SetLoggedType( GetType() );

            // store configuration backups
            _prevUserConfig = _userConfig.Copy();
            _cachedAppConfig = new CachedAppConfig( _appConfig );
        }

        #region Messaging

        protected override void OnActivated()
        {
            base.OnActivated();

            Messenger.Register<DesignTimeOptionsViewModel, SettingsChangedMessage, string>( this, 
                "primary",
                SettingsChangedMessageHandler );
        }

        private void SettingsChangedMessageHandler( DesignTimeOptionsViewModel recipient, SettingsChangedMessage scMesg )
        {
            SettingsChanged = true;
        }

        protected override void OnDeactivated()
        {
            base.OnDeactivated();

            Messenger.UnregisterAll( this );
        }

        #endregion

        public bool SettingsChanged
        {
            get => _settingsChanged;
            private set => SetProperty( ref _settingsChanged, value );
        }

        public ICommand SaveCommand { get; }
        public ICommand ReloadCommand { get; }
    }
}
