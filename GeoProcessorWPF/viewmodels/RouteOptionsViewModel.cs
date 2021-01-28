using System.Windows.Media;
using J4JSoftware.Logging;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace J4JSoftware.GeoProcessor
{
    public class RouteOptionsViewModel : ObservableRecipient, IRouteOptionsViewModel
    {
        private readonly IAppConfig _appConfig;
        private readonly IJ4JLogger? _logger;

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
