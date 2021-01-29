using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using J4JSoftware.Logging;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using Microsoft.Toolkit.Mvvm.Messaging.Messages;
using Microsoft.Win32;

namespace J4JSoftware.GeoProcessor
{
    public class FileViewModel : ObservableRecipient, IFileViewModel
    {
        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

        private readonly FileViewModelValidator _validator = new();
        private readonly IJ4JLogger? _logger;

        private IAppConfig _appConfig;
        private string _inputPath = string.Empty;
        private ExportType _exportType = ExportType.Unknown;
        private string _outputPath = string.Empty;
        private ProcessorType _snapProc;
        private Dictionary<string, List<string>>? _errors;

        public FileViewModel( 
            IAppConfig appConfig, 
            IUserConfig userConfig,
            IJ4JLogger? logger )
        {
            _appConfig = appConfig;

            _logger = logger;
            _logger?.SetLoggedType( GetType() );

            ExportTypes = new ObservableCollection<ExportType>( Enum.GetValues<ExportType>()
                .Where( x => x != ExportType.Unknown ) );

            InitSnappingTypes(userConfig);

            SelectedSnappingType = SnappingTypes!.Any() ? SnappingTypes.First() : ProcessorType.Undefined;

            OutputPath = _appConfig.OutputFile.FilePath;

            Validate( "SnappingTypes" );

            InputFileCommand = new RelayCommand( InputFileDialog );
            OutputFileCommand = new RelayCommand( OutputFileDialog );

            // go live for messages
            IsActive = true;
        }

        private void InitSnappingTypes( IUserConfig userConfig )
        {
            SnappingTypes = new ObservableCollection<ProcessorType>( _appConfig.Processors
                .Where( kvp =>
                    kvp.Value.SupportsSnapping
                    && ( !kvp.Value.RequiresKey
                         || ( userConfig.APIKeys.ContainsKey( kvp.Key )
                              && !string.IsNullOrEmpty( userConfig.APIKeys[ kvp.Key ].Value ) ) ) )
                .Select( kvp => kvp.Key ) );

            OnPropertyChanged( nameof(SnappingTypes) );
        }

        protected override void OnActivated()
        {
            base.OnActivated();

            Messenger.Register<FileViewModel, SettingsReloadedMessage, string>( this, 
                "primary",
                SettingsReloadedMessageHandler );
        }

        protected override void OnDeactivated()
        {
            base.OnDeactivated();

            Messenger.UnregisterAll(this);
        }

        private void SettingsReloadedMessageHandler( FileViewModel recipient, SettingsReloadedMessage srMesg )
        {
            _appConfig = srMesg.AppConfig;

            InitSnappingTypes(srMesg.UserConfig);
            OnPropertyChanged( nameof(SnappingTypes) );

            Validate( "SnappingTypes" );
        }

        public ICommand InputFileCommand { get; }

        private void InputFileDialog()
        {
            var dlg = new OpenFileDialog
            {
                AddExtension = true,
                CheckFileExists = true,
                CheckPathExists = true,
                Filter = "GPS Exchange (*.gpx)|*.gpx|Keyhole Markup Language (*.kml)|*.kml|Keyhole Markup Language (compressed) (*.kmz)|*.kmz",
                Title = "Select the file to process"
            };

            if( !dlg.ShowDialog() ?? false )
                return;

            InputPath = dlg.FileName;
        }

        public string InputPath
        {
            get => _inputPath;

            private set
            {
                SetProperty( ref _inputPath, value );

                _appConfig.InputFile.FilePath = value;

                Validate();
            }
        }

        public ICommand OutputFileCommand { get; }

        private void OutputFileDialog()
        {
            var dlg = new SaveFileDialog
            {
                AddExtension = true,
                CheckFileExists = true,
                CheckPathExists = true,
                Filter = "Keyhole Markup Language (*.kml)|*.kml|Keyhole Markup Language (compressed) (*.kmz)|*.kmz",
                Title = "Select the file for the processed output"
            };

            if( !dlg.ShowDialog() ?? false )
                return;

            OutputPath = dlg.FileName;
        }

        public string OutputPath
        {
            get => _outputPath;

            private set
            {
                _appConfig.OutputFile.FilePath = value;

                if( !value.Equals( _appConfig.OutputFile.FilePath, StringComparison.OrdinalIgnoreCase ) )
                {
                    var mesg = "Unsupported export file type, changed to KML";

                    DisplayMessageAsync( mesg, "Warning" );

                    _logger?.Error( mesg );

                    SetProperty( ref _outputPath, _appConfig.OutputFile.FilePath );
                    
                    return;
                }

                SelectedExportType = _appConfig.OutputFile.Type;

                if( SelectedExportType != ExportType.Unknown )
                {
                    SetProperty( ref _outputPath, _appConfig.OutputFile.FilePath );

                    return;
                }

                var mesg2 = "Unsupported export file type, changing to KML";
                DisplayMessageAsync( mesg2, "Warning" );

                _logger?.Error( mesg2 );

                _appConfig.OutputFile.Type = ExportType.KML;

                SetProperty( ref _outputPath, _appConfig.OutputFile.FilePath );
            }
        }

        public ObservableCollection<ExportType> ExportTypes { get; }

        public ExportType SelectedExportType
        {
            get => _exportType;

            set
            {
                SetProperty( ref _exportType, value );

                _appConfig.ExportType = value;

                SetProperty( ref _outputPath, _appConfig.OutputFile.FilePath );
                OnPropertyChanged( nameof(OutputPath) );
            }
        }

        public ObservableCollection<ProcessorType> SnappingTypes { get; private set; } 

        public ProcessorType SelectedSnappingType
        {
            get => _snapProc;
            set => SetProperty( ref _snapProc, value );
        }

        private async void DisplayMessageAsync( string message, string dlgTitle = "GeoProcessor" )
        {
            var mainWin = Application.Current.MainWindow as MetroWindow;
            
            await mainWin!.ShowMessageAsync( dlgTitle, message );
        }

        public IEnumerable GetErrors( string? propertyName ) =>
            _errors?
                .Where( kvp => string.IsNullOrEmpty( propertyName )
                               || kvp.Key.Equals( propertyName,
                                   StringComparison.Ordinal ) )
                .SelectMany( kvp => kvp.Value )
            ?? Enumerable.Empty<string>();

        public bool HasErrors => _errors?.Any() ?? false;

        private void Validate( [CallerMemberName] string propName = "" )
        {
            _errors = _validator.Validate( this )
                .Errors
                .GroupBy( x => x.PropertyName, x => x.ErrorMessage )
                .ToDictionary( x => x.Key, x => x.ToList() );

            ErrorsChanged?.Invoke( this, new DataErrorsChangedEventArgs( propName ) );

            Messenger.Send( new FileConfigurationMessage( !_errors.Any() ), "primary" );
        }
    }
}
