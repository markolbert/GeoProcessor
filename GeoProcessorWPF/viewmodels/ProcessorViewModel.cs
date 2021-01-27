using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using J4JSoftware.Logging;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace J4JSoftware.GeoProcessor
{
    public class ProcessorViewModel : ObservableRecipient
    {
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
        }

        public ObservableCollection<ProcessorType> ProcessorTypes { get; }

        public ProcessorType SelectedProcessorType
        {
            get => _processorType;

            set
            {
                SetProperty( ref _processorType, value );

                _appConfig.ProcessorType = value;

                // we set some properties indirectly because we don't want to do an update of
                // the underlying model's value, which is what calling the setters would do
                if( _appConfig.Processors.TryGetValue( _processorType, out var processorInfo ) )
                {
                    APIKeyVisible = processorInfo.RequiresKey ? Visibility.Visible : Visibility.Collapsed;
                    RequestLimitVisibility = processorInfo.HasPointsLimit ? Visibility.Visible : Visibility.Collapsed;

                    SetProperty( ref _maxPtsPerReq, processorInfo.MaxPointsPerRequest );
                    SetProperty( ref _maxDistMultiplier, processorInfo.MaxDistanceMultiplier );
                    SetProperty( ref _distanceValue, processorInfo.MaxSeparation.OriginalValue );
                    SetProperty( ref _selectedUnitType, processorInfo.MaxSeparation.Unit );

                    if( _userConfig.APIKeys.TryGetValue( _processorType, out var apiKey ) )
                    {
                        SetProperty( ref _apiKey, apiKey.Value );
                        EncryptedAPIKey = apiKey.EncryptedValue;
                    }
                    else
                    {
                        SetProperty( ref _apiKey, string.Empty );
                        EncryptedAPIKey = string.Empty;
                    }
                }
                else
                {
                    APIKeyVisible = Visibility.Collapsed;
                    RequestLimitVisibility=Visibility.Collapsed;

                    SetProperty( ref _maxPtsPerReq, 100 );
                    SetProperty( ref _maxDistMultiplier, 3 );
                    SetProperty( ref _distanceValue, 2.0 );
                    SetProperty( ref _selectedUnitType, GeoProcessor.UnitTypes.km );
                }
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

                _userConfig.APIKeys[ SelectedProcessorType ].Value = value;

                EncryptedAPIKey = _userConfig.APIKeys[ SelectedProcessorType ].EncryptedValue;
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
            set => SetProperty( ref _maxDistMultiplier, value );
        }

        public Visibility RequestLimitVisibility
        {
            get => _requestLimitVisibility;
            private set => SetProperty( ref _requestLimitVisibility, value );
        }

        public int MaxPointsPerRequest
        {
            get => _maxPtsPerReq;
            set => SetProperty( ref _maxPtsPerReq, value );
        }

        public ObservableCollection<UnitTypes> UnitTypes { get; }

        public UnitTypes SelectedUnitType
        {
            get => _selectedUnitType;

            set
            {
                SetProperty( ref _selectedUnitType, value );

                _appConfig.Processors[ SelectedProcessorType ].MaxSeparation.ChangeUnitType( value );
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

                _appConfig.Processors[ SelectedProcessorType ].MaxSeparation.ChangeOriginalValue( value );
            }
        }
    }
}
