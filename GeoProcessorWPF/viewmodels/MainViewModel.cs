using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using J4JSoftware.Logging;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging.Messages;
using Microsoft.Win32;

namespace J4JSoftware.GeoProcessor
{
    public class MainViewModel : ObservableRecipient
    {
        private readonly IAppConfig _appConfig;
        private readonly IJ4JLogger? _logger;

        private string _inputPath = string.Empty;
        private Visibility _inputWarningVisibility = Visibility.Collapsed;
        private ExportType _exportType = ExportType.Unknown;
        private string _outputPath = string.Empty;
        private int _routeWidth;
        private Color _routeColor;
        private Color _highlightColor;

        public MainViewModel(
            IAppConfig appConfig,
            IJ4JLogger? logger )
        {
            _appConfig = appConfig;

            _logger = logger;
            _logger?.SetLoggedType( GetType() );

            ExportTypes = new ObservableCollection<ExportType>( Enum.GetValues<ExportType>()
                .Where( x => x != ExportType.Unknown ) );

            OutputPath = _appConfig.OutputFile.FilePath;

            RouteWidth = 4;
            RouteColor = _appConfig.RouteColor.ToMediaColor();
            RouteHighlightColor = _appConfig.RouteHighlightColor.ToMediaColor();

            InputFileCommand = new RelayCommand( InputFileDialog );
            OutputFileCommand = new RelayCommand( OutputFileDialog );
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

        public string InputPath
        {
            get => _inputPath;

            private set
            {
                SetProperty( ref _inputPath, value );

                _appConfig.InputFile.FilePath = value;

                InputWarningVisibility = _appConfig.InputFile.Type == ImportType.Unknown
                    ? Visibility.Visible
                    : Visibility.Collapsed;

                if( InputWarningVisibility != Visibility.Visible )
                    _logger?.Error( "Unsupported input file type" );
            }
        }

        public Visibility InputWarningVisibility
        {
            get => _inputWarningVisibility;
            private set => SetProperty( ref _inputWarningVisibility, value );
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
            }
        }

        public int RouteWidth
        {
            get => _routeWidth;

            set
            {
                SetProperty( ref _routeWidth, value );

                _appConfig.RouteWidth = value;
            }
        }

        public Color RouteColor
        {
            get => _routeColor;

            set
            {
                SetProperty( ref _routeColor, value );

                _appConfig.RouteColor = value.ToDrawingColor();
            }
        }

        public Color RouteHighlightColor
        {
            get => _highlightColor;

            set
            {
                SetProperty( ref _highlightColor, value );

                _appConfig.RouteHighlightColor = value.ToDrawingColor();
            }
        }

        private async void DisplayMessageAsync( string message, string dlgTitle = "GeoProcessor" )
        {
            var mainWin = Application.Current.MainWindow as MetroWindow;
            
            await mainWin!.ShowMessageAsync( dlgTitle, message );
        }
    }
}
