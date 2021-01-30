using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Autofac.Features.Indexed;
using J4JSoftware.DependencyInjection;
using J4JSoftware.Logging;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using Microsoft.Win32;
using Twilio.TwiML.Messaging;

namespace J4JSoftware.GeoProcessor
{
    public class OptionsViewModel : ObservableRecipient
    {
        private readonly IJ4JLogger? _logger;

        private IAppConfig _appConfig;
        private CachedAppConfig _cachedAppConfig;
        private IUserConfig _userConfig;
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

            // go live for messages
            IsActive = true;

            // store configuration backups
            _prevUserConfig = _userConfig.Copy();
            _cachedAppConfig = new CachedAppConfig( _appConfig );
        }

        #region messages

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

            Messenger.Send( new SettingsReloadedMessage( _appConfig, _userConfig ), "primary" );

            SettingsChanged = false;
        }

        #endregion
    }
}
