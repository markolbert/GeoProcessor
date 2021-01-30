using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using J4JSoftware.Logging;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
#pragma warning disable 8618

namespace J4JSoftware.GeoProcessor
{
    public class DesignTimeMainViewModel : ObservableRecipient, IMainViewModel
    {
        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged
        {
            add { }
            remove { }
        }

        private IAppConfig _appConfig;
        private bool _configIsValid;
        private bool _settingsChanged;
        private ExportType _exportType = ExportType.Unknown;

        public DesignTimeMainViewModel( IAppConfig appConfig )
        {
            _appConfig = appConfig;

            ExportTypes = new ObservableCollection<ExportType>( Enum.GetValues<ExportType>()
                .Where( x => x != ExportType.Unknown ) );
            
            SelectedExportType = _appConfig.ExportType;
            ConfigurationIsValid = true;

            for( var idx = 0; idx < 10; idx++ )
            {
                Messages.Add( $"Message #{idx + 1}" );
            }
        }

        public ICommand InputFileCommand { get; }
        public string InputPath => "...some input file...";
        public ICommand OutputFileCommand { get; }
        public string OutputPath => "...some output file...";
        
        public ObservableCollection<ExportType> ExportTypes { get; }

        public ExportType SelectedExportType
        {
            get => _exportType;

            set
            {
                SetProperty( ref _exportType, value );

                _appConfig.ExportType = value;
            }
        }

        public bool ConfigurationIsValid
        {
            get => _configIsValid;
            private set => SetProperty( ref _configIsValid, value );
        }

        public ObservableCollection<string> Messages { get; } = new();

        public bool SettingsChanged
        {
            get => _settingsChanged;
            private set => SetProperty( ref _settingsChanged, value );
        }

        public ICommand ProcessCommand { get; }
        public ICommand EditOptionsCommand { get; }
        public IEnumerable GetErrors( string? propertyName ) => Enumerable.Empty<string>();

        public bool HasErrors => false;
    }
}
