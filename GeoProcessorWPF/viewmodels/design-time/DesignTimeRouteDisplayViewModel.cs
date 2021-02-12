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
    public class DesignTimeRouteDisplayViewModel : ObservableRecipient, IRouteDisplayViewModel
    {
        private Color _highlightColor;
        private Color _routeColor;
        private int _routeWidth;

        public DesignTimeRouteDisplayViewModel()
        {
            RouteWidth = 4;
            RouteColor = System.Drawing.Color.Red.ToMediaColor();
            RouteHighlightColor = System.Drawing.Color.DarkTurquoise.ToMediaColor();
        }

        public int RouteWidth
        {
            get => _routeWidth;
            set => SetProperty( ref _routeWidth, value );
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