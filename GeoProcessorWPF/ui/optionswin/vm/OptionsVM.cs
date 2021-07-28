#region license

// Copyright 2021 Mark A. Olbert
// 
// This library or program 'GeoProcessorWPF' is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public License as
// published by the Free Software Foundation, either version 3 of the License,
// or (at your option) any later version.
// 
// This library or program is distributed in the hope that it will be useful, but
// WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// General Public License for more details.
// 
// You should have received a copy of the GNU General Public License along with
// this library or program.  If not, see <https://www.gnu.org/licenses/>.

#endregion

using System.IO;
using System.Text.Json;
using System.Windows.Input;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Extensions.Options;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;

namespace J4JSoftware.GeoProcessor
{
    public class OptionsVM : ObservableRecipient
    {
        private readonly IAppConfig _appConfig;
        private readonly IUserConfig _userConfig;

        private CachedAppConfig _cachedAppConfig;
        private UserConfig _prevUserConfig;

        private bool _settingsChanged;

        public OptionsVM(
            IAppConfig appConfig,
            IUserConfig userConfig
        )
        {
            _appConfig = appConfig;
            _userConfig = userConfig;

            SaveCommand = new RelayCommand( SaveCommandHandlerAsync );
            ReloadCommand = new RelayCommand( ReloadCommandHandler );
            CloseCommand = new RelayCommand<OptionsWindow>( CloseCommandHandler );

            // go live for messages
            IsActive = true;

            // store configuration backups
            _prevUserConfig = _userConfig.Copy();
            _cachedAppConfig = new CachedAppConfig( _appConfig );

            SettingsChanged = false;
        }

        // this constructor is intended for use at design-time only
#pragma warning disable 8618
        public OptionsVM()
#pragma warning restore 8618
        {
            _appConfig = new MockAppConfig();
            _userConfig = new MockUserConfig();

            SettingsChanged = false;
        }

        public bool SettingsChanged
        {
            get => _settingsChanged;
            private set => SetProperty( ref _settingsChanged, value );
        }

        #region Messaging

        protected override void OnActivated()
        {
            base.OnActivated();

            Messenger.Register<OptionsVM, SettingsChangedMessage, string>( this,
                "primary",
                SettingsChangedMessageHandler );
        }

        private void SettingsChangedMessageHandler( OptionsVM recipient, SettingsChangedMessage scMesg )
        {
            SettingsChanged = true;
        }

        protected override void OnDeactivated()
        {
            base.OnDeactivated();

            Messenger.UnregisterAll( this );
        }

        #endregion

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

        private void CloseCommandHandler( OptionsWindow? optionWin )
        {
            if( SettingsChanged )
            {
                var dlgResult = optionWin.ShowModalMessageExternal( "Unsaved Changes",
                    "There are unsaved changes. Are you sure you want to close?",
                    MessageDialogStyle.AffirmativeAndNegative );

                if( dlgResult != MessageDialogResult.Affirmative )
                    return;
            }

            Messenger.Send( new CloseModalWindowMessage( DialogWindow.Options ), "primary" );
        }

        #endregion
    }
}