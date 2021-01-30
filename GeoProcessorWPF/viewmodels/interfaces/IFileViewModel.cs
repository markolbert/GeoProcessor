using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace J4JSoftware.GeoProcessor
{
    public interface IFileViewModel : INotifyDataErrorInfo, INotifyPropertyChanged, INotifyPropertyChanging
    {
        // file specification
        ICommand InputFileCommand { get; }
        string InputPath { get; }
        ICommand OutputFileCommand { get; }
        string OutputPath { get; }
        ObservableCollection<ExportType> ExportTypes { get; }
        ExportType SelectedExportType { get; set; }

        // processing output
        string Phase { get; set; }
        int PointsProcessed { get; }
        ObservableCollection<string> Messages { get; }
        ObservableCollection<ProcessorType> SnappingTypes { get; }
        ProcessorType SelectedSnappingType { get; set; }
    }
}