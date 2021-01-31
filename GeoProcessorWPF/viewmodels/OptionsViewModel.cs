using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using J4JSoftware.Logging;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;

namespace J4JSoftware.GeoProcessor
{
    public class OptionsViewModel : ObservableRecipient, IOptionsViewModel
    {
        private readonly IAppConfig _appConfig;
        private readonly IUserConfig _userConfig;
        private readonly IJ4JLogger? _logger;

        private CachedAppConfig _cachedAppConfig;
        private UserConfig _prevUserConfig;

        private bool _settingsChanged;

        public OptionsViewModel( 
            IAppConfig appConfig,
            IUserConfig userConfig,
            IJ4JLogger? logger 
            )
        {
            _appConfig = appConfig;
            _userConfig = userConfig;

            _logger = logger;
            _logger?.SetLoggedType( GetType() );

            SaveCommand = new RelayCommand( SaveCommandHandlerAsync );
            ReloadCommand = new RelayCommand( ReloadCommandHandler );
            CloseCommand = new RelayCommand<OptionsWindow>( CloseCommandHandler);

            // go live for messages
            IsActive = true;

            // store configuration backups
            _prevUserConfig = _userConfig.Copy();
            _cachedAppConfig = new CachedAppConfig( _appConfig );

            SettingsChanged = false;
        }

        #region Messaging

        protected override void OnActivated()
        {
            base.OnActivated();

            Messenger.Register<OptionsViewModel, SettingsChangedMessage, string>( this, 
                "primary",
                SettingsChangedMessageHandler );
        }

        private void SettingsChangedMessageHandler( OptionsViewModel recipient, SettingsChangedMessage scMesg )
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

        #region Commands

        public ICommand SaveCommand { get; }

        private async void SaveCommandHandlerAsync()
        {
            var options = new JsonSerializerOptions { WriteIndented = true };

            var userText = JsonSerializer.Serialize( _userConfig, options );

            await File.WriteAllTextAsync(
                Path.Combine( _appConfig.UserConfigurationFolder, CompositionRoot.UserConfigFile ),
                userText );

            _prevUserConfig = _userConfig.Copy();

            var appText = JsonSerializer.Serialize( _appConfig, options );

            await File.WriteAllTextAsync(
                Path.Combine( _appConfig.ApplicationConfigurationFolder, CompositionRoot.AppConfigFile ),
                appText );

            _cachedAppConfig = new CachedAppConfig( _appConfig );

            SettingsChanged = false;
        }

        public ICommand ReloadCommand { get; }

        private void ReloadCommandHandler()
        {
            _appConfig.RestoreFrom( _cachedAppConfig );
            _userConfig.RestoreFrom( _prevUserConfig );

            SettingsChanged = false;
        }

        public ICommand CloseCommand { get; }

        private void CloseCommandHandler( OptionsWindow optionWin )
        {
            if( SettingsChanged )
            {
                var dlgResult = optionWin.ShowModalMessageExternal( "Unsaved Changes",
                    "There are unsaved changes. Are you sure you want to close?",
                    MessageDialogStyle.AffirmativeAndNegative );

                if( dlgResult != MessageDialogResult.Affirmative )
                    return;
            }

            Messenger.Send( new OptionsWindowClosed(), "primary" );
        }

        #endregion

    }
}
