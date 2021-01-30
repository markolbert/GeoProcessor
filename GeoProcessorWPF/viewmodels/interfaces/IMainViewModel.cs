using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace J4JSoftware.GeoProcessor
{
    public interface IMainViewModel : INotifyDataErrorInfo, INotifyPropertyChanged, INotifyPropertyChanging
    {
        // file specification
        ICommand InputFileCommand { get; }
        string InputPath { get; }
        ICommand OutputFileCommand { get; }
        string OutputPath { get; }
        ObservableCollection<ExportType> ExportTypes { get; }
        ExportType SelectedExportType { get; set; }

        // status
        bool ConfigurationIsValid { get; }
        ObservableCollection<string> Messages { get; }

        // commands
        ICommand EditOptionsCommand { get; }
        ICommand ProcessCommand { get; }
    }
}