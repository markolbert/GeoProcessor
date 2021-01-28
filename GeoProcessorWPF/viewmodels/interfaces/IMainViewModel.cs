using System.ComponentModel;
using System.Windows.Input;

namespace J4JSoftware.GeoProcessor
{
    public interface IMainViewModel : INotifyPropertyChanged, INotifyPropertyChanging
    {
        bool ConfigurationIsValid { get; }
        ICommand SaveCommand { get; }
        ICommand ProcessCommand { get; }
    }
}