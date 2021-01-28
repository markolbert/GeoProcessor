using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using J4JSoftware.Logging;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Win32;

namespace J4JSoftware.GeoProcessor
{
    public class DesignTimeFileViewModel : ObservableRecipient, IFileViewModel
    {
        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

        private string _inputPath = string.Empty;
        private ExportType _exportType = ExportType.Unknown;
        private string _outputPath = string.Empty;
        private ProcessorType _snapProc;
        private Dictionary<string, List<string>>? _errors;

        public DesignTimeFileViewModel()
        {
            foreach( var exportType in Enum.GetValues<ExportType>() )
            {
                ExportTypes.Add( exportType );
            }

            SnappingTypes.Add(ProcessorType.Bing  );
            SnappingTypes.Add( ProcessorType.Google );
            SelectedSnappingType = ProcessorType.Bing;

            InputPath = "...some input file path...";
            OutputPath = "...some output file path...";
        }

        public ICommand InputFileCommand { get; }
        public ICommand OutputFileCommand { get; }

        public string InputPath
        {
            get => _inputPath;
            private set =>SetProperty( ref _inputPath, value );
        }

        public string OutputPath
        {
            get => _outputPath;
            private set => SetProperty( ref _outputPath, value );
        }

        public ObservableCollection<ExportType> ExportTypes { get; } = new();

        public ExportType SelectedExportType
        {
            get => _exportType;
            set =>SetProperty( ref _exportType, value );
        }

        public ObservableCollection<ProcessorType> SnappingTypes { get; } = new();

        public ProcessorType SelectedSnappingType
        {
            get => _snapProc;
            set => SetProperty( ref _snapProc, value );
        }

        public IEnumerable GetErrors( string? propertyName ) => Enumerable.Empty<string>();
        public bool HasErrors => _errors?.Any() ?? false;
    }
}
