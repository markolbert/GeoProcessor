﻿using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using J4JSoftware.Logging;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace J4JSoftware.GeoProcessor
{
    public class RouteEnginesViewModel : ObservableRecipient, IRouteEnginesViewModel
    {
        private enum PropertySettingState
        {
            Initial,
            ProcessorTypeChange,
            Normal
        }

        private readonly IAppConfig _appConfig;
        private readonly IUserConfig _userConfig;

        private Visibility _apiKeyVisibility = Visibility.Collapsed;
        private ProcessorType _processorType = ProcessorType.None;
        private string _apiKey = string.Empty;
        private string _encyptedApiKey = string.Empty;
        private int _maxDistMultiplier;
        private UnitTypes _selectedUnitType;
        private double _distanceValue;
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
            private set => SetProperty(ref _encyptedApiKey, value  );
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
                Messenger.Send( new SettingsChangedMessage(SettingsPage.Processors), "primary" );
        }
    }
}
