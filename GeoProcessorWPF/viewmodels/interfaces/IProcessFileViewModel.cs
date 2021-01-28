using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace J4JSoftware.GeoProcessor
{
    public interface IProcessFileViewModel : INotifyPropertyChanged, INotifyPropertyChanging
    {
        string Phase { get; set; }
        int PointsProcessed { get; }
        ObservableCollection<string> Messages { get; }
        string CommandButtonText { get; }
        Visibility CancelVisibility {get; }
        ICommand CancelCommand { get; }
        ICommand ProcessCommand { get; }
    }
}