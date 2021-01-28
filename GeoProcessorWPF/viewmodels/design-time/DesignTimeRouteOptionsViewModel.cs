using System.Windows.Media;
using J4JSoftware.Logging;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace J4JSoftware.GeoProcessor
{
    public class DesignTimeRouteOptionsViewModel : ObservableRecipient, IRouteOptionsViewModel
    {
        private int _routeWidth;
        private Color _routeColor;
        private Color _highlightColor;

        public DesignTimeRouteOptionsViewModel()
        {
            RouteWidth = 4;
            RouteColor = System.Drawing.Color.Red.ToMediaColor();
            RouteHighlightColor = System.Drawing.Color.DarkTurquoise.ToMediaColor();
        }

        public int RouteWidth
        {
            get => _routeWidth;
            set =>SetProperty( ref _routeWidth, value );
        }

        public Color RouteColor
        {
            get => _routeColor;
            set => SetProperty( ref _routeColor, value );
        }

        public Color RouteHighlightColor
        {
            get => _highlightColor;
            set => SetProperty( ref _highlightColor, value );
        }
    }
}
