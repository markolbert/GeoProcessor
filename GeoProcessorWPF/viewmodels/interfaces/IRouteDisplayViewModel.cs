using System.ComponentModel;
using System.Windows.Media;

namespace J4JSoftware.GeoProcessor
{
    public interface IRouteDisplayViewModel : INotifyPropertyChanged, INotifyPropertyChanging
    {
        int RouteWidth { get; set; }
        Color RouteColor { get; set; }
        Color RouteHighlightColor { get; set; }
    }
}