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
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Toolkit.Mvvm.ComponentModel;

#pragma warning disable 8618

namespace J4JSoftware.GeoProcessor
{
    public class DesignTimeMainViewModel : ObservableRecipient, IMainViewModel
    {
        private readonly MainViewValidator _validator = new();

        private readonly IAppConfig _appConfig;
        private bool _configIsValid;

        private Dictionary<string, List<string>>? _errors;
        private ExportType _exportType = ExportType.Unknown;
        private bool _settingsChanged;

        public DesignTimeMainViewModel( IAppConfig appConfig )
        {
            _appConfig = appConfig;

            ExportTypes = new ObservableCollection<ExportType>( Enum.GetValues<ExportType>()
                .Where( x => x != ExportType.Unknown ) );

            SelectedExportType = _appConfig.ExportType;
            ConfigurationIsValid = true;

            Validate();

            for( var idx = 0; idx < 10; idx++ ) Messages.Add( $"Message #{idx + 1}" );
        }

        public bool SettingsChanged
        {
            get => _settingsChanged;
            private set => SetProperty( ref _settingsChanged, value );
        }

        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

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

        public ICommand ProcessCommand { get; }
        public ICommand EditOptionsCommand { get; }
        public ICommand AboutCommand { get; }
        public ICommand HelpCommand { get; }

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

            ErrorsChanged?.Invoke( this, new DataErrorsChangedEventArgs( propName ) );

            ConfigurationIsValid = !Messages.Any();

            //Messenger.Send( new FileConfigurationMessage( !_errors.Any() ), "primary" );
        }

        #endregion
    }
}