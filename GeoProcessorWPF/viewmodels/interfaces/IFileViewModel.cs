using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace J4JSoftware.GeoProcessor
{
    public interface IFileViewModel : INotifyDataErrorInfo, INotifyPropertyChanged, INotifyPropertyChanging
    {
        ICommand InputFileCommand { get; }
        ICommand OutputFileCommand { get; }
        string InputPath { get; }
        string OutputPath { get; }
        ObservableCollection<ExportType> ExportTypes { get; }
        ExportType SelectedExportType { get; set; }
        ObservableCollection<ProcessorType> SnappingTypes { get; }
        ProcessorType SelectedSnappingType { get; set; }
    }
}