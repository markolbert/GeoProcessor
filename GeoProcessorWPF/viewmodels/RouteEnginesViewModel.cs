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
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace J4JSoftware.GeoProcessor
{
    public class RouteEnginesViewModel : ObservableRecipient, IRouteEnginesViewModel
    {
        private readonly IAppConfig _appConfig;
        private readonly IUserConfig _userConfig;
        private string _apiKey = string.Empty;

        private Visibility _apiKeyVisibility = Visibility.Collapsed;
        private double _distanceValue;
        private string _encyptedApiKey = string.Empty;
        private int _maxDistMultiplier;
        private ProcessorType _processorType = ProcessorType.None;
        private UnitTypes _selectedUnitType;
        private PropertySettingState _setState;

        public RouteEnginesViewModel(
            IAppConfig appConfig,
            IUserConfig userConfig )
        {
            _appConfig = appConfig;
            _userConfig = userConfig;

            ProcessorTypes = new ObservableCollection<ProcessorType>( Enum.GetValues<ProcessorType>()
                .Where( x => x != ProcessorType.None ) );

            UnitTypes = new ObservableCollection<UnitTypes>( Enum.GetValues<UnitTypes>() );
            SelectedProcessorType = ProcessorTypes.FirstOrDefault( x => x != ProcessorType.Distance );

            _setState = PropertySettingState.Normal;
        }

        public ObservableCollection<ProcessorType> ProcessorTypes { get; }

        public ProcessorType SelectedProcessorType
        {
            get => _processorType;

            set
            {
                SetProperty( ref _processorType, value );

                // don't update the underlying AppConfig object with the new values 
                if( _setState == PropertySettingState.Normal )
                    _setState = PropertySettingState.ProcessorTypeChange;

                OnSettingsChanged();

                APIKeyVisible = _processorType.RequiresAPIKey() ? Visibility.Visible : Visibility.Collapsed;

                if( _appConfig.Processors.TryGetValue( _processorType, out var processorInfo ) )
                {
                    MaxDistanceMultiplier = processorInfo.MaxDistanceMultiplier;
                    DistanceValue = processorInfo.MaxSeparation.OriginalValue;
                    SelectedUnitType = processorInfo.MaxSeparation.Unit;

                    if( _userConfig.APIKeys.TryGetValue( _processorType, out var apiKey ) )
                    {
                        APIKey = apiKey.Value;
                        EncryptedAPIKey = apiKey.EncryptedValue;
                    }
                    else
                    {
                        APIKey = string.Empty;
                        EncryptedAPIKey = string.Empty;
                    }
                }
                else
                {
                    MaxDistanceMultiplier = 3;
                    DistanceValue = 2.0;
                    SelectedUnitType = GeoProcessor.UnitTypes.km;
                }

                if( _setState == PropertySettingState.ProcessorTypeChange )
                    _setState = PropertySettingState.Normal;
            }
        }

        public Visibility APIKeyVisible
        {
            get => _apiKeyVisibility;
            private set => SetProperty( ref _apiKeyVisibility, value );
        }

        public string APIKey
        {
            get => _apiKey;

            set
            {
                SetProperty( ref _apiKey, value );
                OnSettingsChanged();

                if( _setState != PropertySettingState.Normal
                    || !_userConfig.APIKeys.TryGetValue( SelectedProcessorType, out var temp ) )
                    return;

                temp.Value = value;
                EncryptedAPIKey = temp.EncryptedValue;
            }
        }

        public string EncryptedAPIKey
        {
            get => _encyptedApiKey;
            private set => SetProperty( ref _encyptedApiKey, value );
        }

        public int MaxDistanceMultiplier
        {
            get => _maxDistMultiplier;

            set
            {
                SetProperty( ref _maxDistMultiplier, value );
                OnSettingsChanged();

                if( _setState != PropertySettingState.Normal
                    || !_appConfig.Processors.TryGetValue( SelectedProcessorType, out var temp ) )
                    return;

                temp.MaxDistanceMultiplier = value;
            }
        }

        public ObservableCollection<UnitTypes> UnitTypes { get; }

        public UnitTypes SelectedUnitType
        {
            get => _selectedUnitType;

            set
            {
                SetProperty( ref _selectedUnitType, value );
                OnSettingsChanged();

                if( _setState != PropertySettingState.Normal
                    || !_appConfig.Processors.TryGetValue( SelectedProcessorType, out var temp ) )
                    return;

                temp.MaxSeparation.ChangeUnitType( value );
            }
        }

        public double DistanceValue
        {
            get => _distanceValue;

            set
            {
                if( value <= 0 )
                    return;

                SetProperty( ref _distanceValue, value );
                OnSettingsChanged();

                if( _setState != PropertySettingState.Normal
                    || !_appConfig.Processors.TryGetValue( SelectedProcessorType, out var temp ) )
                    return;

                temp.MaxSeparation.ChangeOriginalValue( value );
            }
        }

        private void OnSettingsChanged()
        {
            if( _setState == PropertySettingState.Normal )
                Messenger.Send( new SettingsChangedMessage( SettingsPage.Processors ), "primary" );
        }

        private enum PropertySettingState
        {
            Initial,
            ProcessorTypeChange,
            Normal
        }
    }
}