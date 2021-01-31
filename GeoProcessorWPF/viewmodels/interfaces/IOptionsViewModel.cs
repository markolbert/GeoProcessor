using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Input;

namespace J4JSoftware.GeoProcessor
{
    public interface IOptionsViewModel : INotifyPropertyChanged, INotifyPropertyChanging
    {
        bool SettingsChanged { get; }
        ICommand SaveCommand { get; }
        ICommand ReloadCommand { get; }
    }
}