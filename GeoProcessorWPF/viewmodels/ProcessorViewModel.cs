using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using J4JSoftware.Logging;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace J4JSoftware.GeoProcessor
{
    public class ProcessorViewModel : ObservableRecipient, IProcessorViewModel
    {
        private enum PropertySettingState
        {
            Initial,
            ProcessorTypeChange,
            Normal
        }

        private readonly IAppConfig _appConfig;
        private readonly IUserConfig _userConfig;
        private readonly IJ4JLogger? _logger;

        private Visibility _apiKeyVisibility = Visibility.Collapsed;
        private Visibility _requestLimitVisibility = Visibility.Collapsed;
        private ProcessorType _processorType = ProcessorType.Undefined;
        private string _apiKey = string.Empty;
        private string _encyptedApiKey = string.Empty;
        private int _maxDistMultiplier;
        private int _maxPtsPerReq;
        private UnitTypes _selectedUnitType;
        private double _distanceValue;
        private PropertySettingState _setState = PropertySettingState.Initial;

        public ProcessorViewModel(
            IAppConfig appConfig,
            IUserConfig userConfig,
            IJ4JLogger? logger )
        {
            _appConfig = appConfig;
            _userConfig = userConfig;

            _logger = logger;
            _logger?.SetLoggedType( GetType() );

            ProcessorTypes = new ObservableCollection<ProcessorType>( Enum.GetValues<ProcessorType>()
                .Where( x => x != ProcessorType.Undefined ) );

            SelectedProcessorType = ProcessorTypes.FirstOrDefault( x => x != ProcessorType.Distance );

            UnitTypes = new ObservableCollection<UnitTypes>( Enum.GetValues<UnitTypes>() );

            SelectedUnitType = _appConfig.Processors.TryGetValue( SelectedProcessorType, out var procInfo ) 
                ? procInfo.MaxSeparation.Unit 
                : GeoProcessor.UnitTypes.km;

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

                _appConfig.ProcessorType = value;

                if( _appConfig.Processors.TryGetValue( _processorType, out var processorInfo ) )
                {
                    APIKeyVisible = processorInfo.RequiresKey ? Visibility.Visible : Visibility.Collapsed;
                    RequestLimitVisibility = processorInfo.HasPointsLimit ? Visibility.Visible : Visibility.Collapsed;

                    MaxPointsPerRequest = processorInfo.MaxPointsPerRequest;
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
                    APIKeyVisible = Visibility.Collapsed;
                    RequestLimitVisibility=Visibility.Collapsed;

                    MaxPointsPerRequest = 100;
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

        public Visibility RequestLimitVisibility
        {
            get => _requestLimitVisibility;
            private set => SetProperty( ref _requestLimitVisibility, value );
        }

        public int MaxPointsPerRequest
        {
            get => _maxPtsPerReq;
            set
            {
                SetProperty( ref _maxPtsPerReq, value );
                OnSettingsChanged();

                if( _setState != PropertySettingState.Normal 
                    || !_appConfig.Processors.TryGetValue( SelectedProcessorType, out var temp ) ) 
                    return;

                temp.MaxPointsPerRequest = value;
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
                Messenger.Send( new SettingsChangedMessage(), "primary" );
        }
    }
}
