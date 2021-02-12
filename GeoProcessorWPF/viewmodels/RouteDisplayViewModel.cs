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

using System.Windows.Media;
using J4JSoftware.WPFViewModel;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace J4JSoftware.GeoProcessor
{
    public class RouteDisplayViewModel : ObservableRecipient, IRouteDisplayViewModel
    {
        private IAppConfig _appConfig;
        private Color _highlightColor;
        private Color _routeColor;
        private int _routeWidth;
        private readonly bool _suppressChangeMessages = true;

        public RouteDisplayViewModel( IAppConfig appConfig )
        {
            _appConfig = appConfig;

            RouteWidth = 4;
            RouteColor = _appConfig.RouteColor.ToMediaColor();
            RouteHighlightColor = _appConfig.RouteHighlightColor.ToMediaColor();

            // we didn't want to generate settings changed messages during initial configuration
            _suppressChangeMessages = false;

            // go live for messages
            IsActive = true;
        }

        public int RouteWidth
        {
            get => _routeWidth;

            set
            {
                SetProperty( ref _routeWidth, value );
                OnSettingsChanged();

                _appConfig.RouteWidth = value;
            }
        }

        public Color RouteColor
        {
            get => _routeColor;

            set
            {
                SetProperty( ref _routeColor, value );
                OnSettingsChanged();

                _appConfig.RouteColor = value.ToDrawingColor();
            }
        }

        public Color RouteHighlightColor
        {
            get => _highlightColor;

            set
            {
                SetProperty( ref _highlightColor, value );
                OnSettingsChanged();

                _appConfig.RouteHighlightColor = value.ToDrawingColor();
            }
        }

        protected override void OnActivated()
        {
            base.OnActivated();

            Messenger.Register<RouteDisplayViewModel, SettingsReloadedMessage, string>( this,
                "primary",
                SettingsReloadedMessageHandler );
        }

        protected override void OnDeactivated()
        {
            base.OnDeactivated();

            Messenger.UnregisterAll( this );
        }

        private void SettingsReloadedMessageHandler( RouteDisplayViewModel recipient, SettingsReloadedMessage srMesg )
        {
            _appConfig = srMesg.AppConfig;

            RouteWidth = srMesg.AppConfig.RouteWidth;
            RouteColor = srMesg.AppConfig.RouteColor.ToMediaColor();
            RouteHighlightColor = srMesg.AppConfig.RouteHighlightColor.ToMediaColor();
        }

        private void OnSettingsChanged()
        {
            if( !_suppressChangeMessages )
                Messenger.Send( new SettingsChangedMessage( SettingsPage.RouteDisplay ), "primary" );
        }
    }
}