#region license

// Copyright 2021 Mark A. Olbert
// 
// This library or program 'GeoProcessorWPF' is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public License as
// published by the Free Software Foundation, either version 3 of the License,
// or (at your option) any later version.
// 
// This library or program is distributed in the hope that it will be useful, but
// WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// General Public License for more details.
// 
// You should have received a copy of the GNU General Public License along with
// this library or program.  If not, see <https://www.gnu.org/licenses/>.

#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using J4JSoftware.Logging;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Win32;
#pragma warning disable 8618

namespace J4JSoftware.GeoProcessor
{
    public class MainVM : ObservableRecipient
    {
        private readonly IAppConfig _appConfig;
        private readonly IJ4JLogger? _logger;

        private readonly MainVMValidator _validator = new();

        private bool _configIsValid;

        private Dictionary<string, List<string>>? _errors;
        private ExportType _exportType = ExportType.Unknown;

        private string _inputPath = string.Empty;
        private OptionsWindow? _optionsWin;
        private string _outputPath = string.Empty;
        private ProcessWindow? _procWin;
        private ProcessorType _snapType;

        public MainVM(
            IAppConfig appConfig,
            IUserConfig userConfig,
            IJ4JLogger? logger )
        {
            _appConfig = appConfig;

            _logger = logger;
            _logger?.SetLoggedType( GetType() );

            // file specification
            ExportTypes = new ObservableCollection<ExportType>( Enum.GetValues<ExportType>()
                .Where( x => x != ExportType.Unknown ) );
            SelectedExportType = _appConfig.ExportType;

            OutputPath = _appConfig.OutputFile.FilePath;

            InputFileCommand = new RelayCommand( InputFileDialog );
            OutputFileCommand = new RelayCommand( OutputFileDialog );
            EditOptionsCommand = new RelayCommand( EditOptionsCommandHandler );
            ProcessCommand = new RelayCommand( ProcessCommandHandlerAsync );
            AboutCommand = new AsyncRelayCommand( AboutCommandHandler );
            HelpCommand = new RelayCommand( HelpCommandHandler );

            InitSnapToRouteProcessors( userConfig );
            SelectedSnapToRouteProcessor =
                SnapToRouteProcessors!.Any() ? SnapToRouteProcessors!.First() : ProcessorType.None;
            Validate();

            // go live for messages
            IsActive = true;

            Validate();
        }

        // this constructor intended only for design-time use
        public MainVM()
        {
            _appConfig = new MockAppConfig();

            InputPath = _appConfig.InputFile.FilePath;
            OutputPath = _appConfig.OutputFile.FilePath;

            ExportTypes = new ObservableCollection<ExportType>(Enum.GetValues<ExportType>()
                .Where(x => x != ExportType.Unknown));

            var userConfig = new MockUserConfig();

            InitSnapToRouteProcessors(userConfig);
            SelectedSnapToRouteProcessor =
                SnapToRouteProcessors!.Any() ? SnapToRouteProcessors!.First() : ProcessorType.None;

            SelectedExportType = ExportType.KML;
            ConfigurationIsValid = true;

            for( var idx = 0; idx < 3; idx++ )
            {
                Messages.Add($"Message #{idx + 1}");
            }
        }

        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

        public bool ConfigurationIsValid
        {
            get => _configIsValid;
            private set => SetProperty( ref _configIsValid, value );
        }

        public ObservableCollection<ProcessorType> SnapToRouteProcessors { get; private set; }

        public ProcessorType SelectedSnapToRouteProcessor
        {
            get => _snapType;

            set
            {
                SetProperty( ref _snapType, value );
                _appConfig.ProcessorType = value;

                Validate();
            }
        }

        public ObservableCollection<string> Messages { get; private set; } = new();

        private void InitSnapToRouteProcessors( IUserConfig userConfig )
        {
            SnapToRouteProcessors = new ObservableCollection<ProcessorType>( Enum.GetValues<ProcessorType>()
                .Where( x =>
                    x.SnapsToRoute()
                    && ( !x.RequiresAPIKey()
                         || userConfig.APIKeys.TryGetValue( x, out var apiKey ) &&
                         !string.IsNullOrEmpty( apiKey.Value ) ) ) );

            OnPropertyChanged( nameof(SnapToRouteProcessors) );
        }

        private async Task DisplayMessageAsync( string message, string dlgTitle = "GeoProcessor" )
        {
            var mainWin = Application.Current.MainWindow as MetroWindow;

            await mainWin!.ShowMessageAsync( dlgTitle, message );
        }

        #region Messaging

        protected override void OnActivated()
        {
            base.OnActivated();

            Messenger.Register<MainVM, ProcessorStateMessage, string>( this,
                "primary",
                ProcessorStateMessageHandler );

            Messenger.Register<MainVM, CloseModalWindowMessage, string>( this,
                "primary",
                CloseModalWindowMessageHandler );
        }

        protected override void OnDeactivated()
        {
            base.OnDeactivated();

            Messenger.UnregisterAll( this );
        }

        private void ProcessorStateMessageHandler( MainVM recipient, ProcessorStateMessage pcMesg )
        {
            switch( pcMesg.ProcessorState )
            {
                case ProcessorState.Ready:
                case ProcessorState.Running:
                    return;

                case ProcessorState.Aborted:
                    DisplayMessageAsync( "Processing aborted", "User Abort" );
                    break;

                case ProcessorState.Finished:
                    DisplayMessageAsync( $"Wrote file '{_appConfig.OutputFile.FilePath}'", "Processing Completed" );
                    break;
            }
        }

        private void CloseModalWindowMessageHandler( MainVM recipient, CloseModalWindowMessage cmwMesg )
        {
            switch( cmwMesg.Window )
            {
                case DialogWindow.Options:
                    _optionsWin?.Close();
                    Validate();

                    break;

                case DialogWindow.Processor:
                    _procWin?.Close();
                    break;
            }
        }

        #endregion

        #region Commands

        public ICommand ProcessCommand { get; }

        private async void ProcessCommandHandlerAsync()
        {
            if( File.Exists( _appConfig.OutputFile.FilePath ) )
            {
                var mainWin = Application.Current.MainWindow as MetroWindow;

                var result = await mainWin!.ShowMessageAsync( "Output File Exists",
                    "The output file exists. Do you want to overwrite it?",
                    MessageDialogStyle.AffirmativeAndNegative );

                if( result == MessageDialogResult.Negative )
                    return;
            }

            _procWin = new ProcessWindow();

            // we receive a message from the dialog window when it closes,
            // and take action at that point
            _procWin.ShowDialog();
        }

        public ICommand EditOptionsCommand { get; }

        private void EditOptionsCommandHandler()
        {
            _optionsWin = new OptionsWindow();

            // we receive a message from the dialog window when it closes,
            // and take action at that point
            _optionsWin.ShowDialog();
        }

        public ICommand AboutCommand { get; }

        private async Task AboutCommandHandler()
        {
            await DisplayMessageAsync( "GeoProcessor v0.8\n\nCopyright 2021 Mark Olbert all rights reserved", "About" );
        }

        public ICommand HelpCommand { get; }

        private void HelpCommandHandler()
        {
            Process.Start( new ProcessStartInfo( "https://jumpforjoysoftware.com/geoprocessor/" )
            {
                UseShellExecute = true
            } );
        }

        #endregion

        #region file specification

        public ICommand InputFileCommand { get; }

        private void InputFileDialog()
        {
            var dlg = new OpenFileDialog
            {
                AddExtension = true,
                CheckFileExists = true,
                CheckPathExists = true,
                Filter =
                    "GPS Exchange (*.gpx)|*.gpx|Keyhole Markup Language (*.kml)|*.kml|Keyhole Markup Language (compressed) (*.kmz)|*.kmz",
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

        #endregion

        #region error handling

        // we always return an empty enumerable because we display the error
        // messages in a different way (i.e., not as annotations)
        public IEnumerable GetErrors( string? propertyName )
        {
            var propHasErrors = _errors?
                                    .Any( kvp => string.IsNullOrEmpty( propertyName )
                                                 || kvp.Key.Equals( propertyName, StringComparison.Ordinal ) )
                                ?? false;

            var retVal = new List<string>();

            if( propHasErrors )
                retVal.Add( string.Empty );

            return retVal;
        }

        public bool HasErrors => _errors?.Any() ?? false;

        private void Validate( [ CallerMemberName ] string propName = "" )
        {
            _errors = _validator.Validate( this )
                .Errors
                .GroupBy( x => x.PropertyName, x => x.ErrorMessage )
                .ToDictionary( x => x.Key, x => x.ToList() );

            Messages = new ObservableCollection<string>( _errors.SelectMany( x => x.Value ) );
            OnPropertyChanged( nameof(Messages) );

            ErrorsChanged?.Invoke( this, new DataErrorsChangedEventArgs( propName ) );

            ConfigurationIsValid = !Messages.Any();
        }

        #endregion
    }
}