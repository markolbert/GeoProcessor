using System.Windows.Media;
using J4JSoftware.Logging;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace J4JSoftware.GeoProcessor
{
    public class RouteOptionsViewModel : ObservableRecipient, IRouteOptionsViewModel
    {
        private readonly IJ4JLogger? _logger;

        private IAppConfig _appConfig;
        private int _routeWidth;
        private Color _routeColor;
        private Color _highlightColor;
        private bool _suppressChangeMessages = true;

        public RouteOptionsViewModel(
            IAppConfig appConfig,
            IJ4JLogger? logger )
        {
            _appConfig = appConfig;

            _logger = logger;
            _logger?.SetLoggedType( GetType() );

            RouteWidth = 4;
            RouteColor = _appConfig.RouteColor.ToMediaColor();
            RouteHighlightColor = _appConfig.RouteHighlightColor.ToMediaColor();

            // we didn't want to generate settings changed messages during initial configuration
            _suppressChangeMessages = false;

            // go live for messages
            IsActive = true;
        }

        protected override void OnActivated()
        {
            base.OnActivated();

            Messenger.Register<RouteOptionsViewModel, SettingsReloadedMessage, string>( this, 
                "primary",
                SettingsReloadedMessageHandler );
        }

        protected override void OnDeactivated()
        {
            base.OnDeactivated();

            Messenger.UnregisterAll(this);
        }

        private void SettingsReloadedMessageHandler( RouteOptionsViewModel recipient, SettingsReloadedMessage srMesg )
        {
            _appConfig = srMesg.AppConfig;

            RouteWidth = srMesg.AppConfig.RouteWidth;
            RouteColor = srMesg.AppConfig.RouteColor.ToMediaColor();
            RouteHighlightColor = srMesg.AppConfig.RouteHighlightColor.ToMediaColor();
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

        private void OnSettingsChanged()
        {
            if( !_suppressChangeMessages )
                Messenger.Send( new SettingsChangedMessage(), "primary" );
        }
    }
}
