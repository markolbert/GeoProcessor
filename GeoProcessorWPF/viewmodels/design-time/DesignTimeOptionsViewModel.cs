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

using System.Windows.Input;
using Microsoft.Toolkit.Mvvm.ComponentModel;

#pragma warning disable 8618

namespace J4JSoftware.GeoProcessor
{
    public class DesignTimeOptionsViewModel : ObservableRecipient, IOptionsViewModel
    {
        private bool _settingsChanged;

        public DesignTimeOptionsViewModel(
            IAppConfig appConfig,
            IUserConfig userConfig
        )
        {
        }

        public bool SettingsChanged
        {
            get => _settingsChanged;
            private set => SetProperty( ref _settingsChanged, value );
        }

        public ICommand SaveCommand { get; }
        public ICommand ReloadCommand { get; }
        public ICommand CloseCommand { get; }

        #region Messaging

        protected override void OnActivated()
        {
            base.OnActivated();

            Messenger.Register<DesignTimeOptionsViewModel, SettingsChangedMessage, string>( this,
                "primary",
                SettingsChangedMessageHandler );
        }

        private void SettingsChangedMessageHandler( DesignTimeOptionsViewModel recipient,
            SettingsChangedMessage scMesg )
        {
            SettingsChanged = true;
        }

        protected override void OnDeactivated()
        {
            base.OnDeactivated();

            Messenger.UnregisterAll( this );
        }

        #endregion
    }
}