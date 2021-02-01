using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
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
        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

        private readonly MainViewValidator _validator = new();

        private IAppConfig _appConfig;
        private bool _configIsValid;
        private bool _settingsChanged;
        private ExportType _exportType = ExportType.Unknown;

        private Dictionary<string, List<string>>? _errors;

        public DesignTimeMainViewModel( IAppConfig appConfig )
        {
            _appConfig = appConfig;

            ExportTypes = new ObservableCollection<ExportType>( Enum.GetValues<ExportType>()
                .Where( x => x != ExportType.Unknown ) );
            
            SelectedExportType = _appConfig.ExportType;
            ConfigurationIsValid = true;

            Validate();

            for( var idx = 0; idx < 10; idx++ )
            {
                Messages.Add( $"Message #{idx + 1}" );
            }
        }

        public ICommand InputFileCommand { get; }
        public string InputPath => "...some input file...";
        public ICommand OutputFileCommand { get; }
        public string OutputPath => "...some output file...";

        public ObservableCollection<ProcessorType> SnapToRouteProcessors { get; } = new();
        public ProcessorType SelectedSnapToRouteProcessor { get; set; } = ProcessorType.None;

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

        private void Validate( [CallerMemberName] string propName = "" )
        {
            _errors = _validator.Validate( this )
                .Errors
                .GroupBy( x => x.PropertyName, x => x.ErrorMessage )
                .ToDictionary( x => x.Key, x => x.ToList() );

            ErrorsChanged?.Invoke( this, new DataErrorsChangedEventArgs( propName ) );

            ConfigurationIsValid = !Messages.Any();

            //Messenger.Send( new FileConfigurationMessage( !_errors.Any() ), "primary" );
        }

        #endregion
    }
}
