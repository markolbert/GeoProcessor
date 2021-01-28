using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace J4JSoftware.GeoProcessor
{
    public interface IProcessFileViewModel
    {
        string Phase { get; set; }
        int PointsProcessed { get; }
        ObservableCollection<string> Messages { get; }
        string CommandButtonText { get; }
        bool Succeeded { get; }
        Visibility CancelVisibility {get; }
        ICommand CancelCommand { get; }
        ICommand ProcessCommand { get; }
        bool IsActive { get; set; }
        event PropertyChangedEventHandler? PropertyChanged;
        event PropertyChangingEventHandler? PropertyChanging;
    }
}