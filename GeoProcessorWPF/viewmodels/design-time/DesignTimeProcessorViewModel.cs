using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using J4JSoftware.Logging;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace J4JSoftware.GeoProcessor
{
    public class DesignTimeProcessorViewModel : ObservableRecipient, IProcessorViewModel
    {
        private Visibility _apiKeyVisibility = Visibility.Collapsed;
        private Visibility _requestLimitVisibility = Visibility.Collapsed;
        private ProcessorType _processorType = ProcessorType.None;
        private string _apiKey = string.Empty;
        private string _encyptedApiKey = string.Empty;
        private int _maxDistMultiplier;
        private UnitTypes _selectedUnitType;
        private double _distanceValue;

        public DesignTimeProcessorViewModel()
        {
            ProcessorTypes.Add( ProcessorType.Bing );
            ProcessorTypes.Add( ProcessorType.Distance );
            ProcessorTypes.Add( ProcessorType.Google );

            SelectedProcessorType = ProcessorType.Bing;
            UnitTypes = new ObservableCollection<UnitTypes>( Enum.GetValues<UnitTypes>() );
            SelectedUnitType = GeoProcessor.UnitTypes.mi;
        }

        public ObservableCollection<ProcessorType> ProcessorTypes { get; } = new();

        public ProcessorType SelectedProcessorType
        {
            get => _processorType;
            set => SetProperty( ref _processorType, value );
        }

        public Visibility APIKeyVisible
        {
            get => _apiKeyVisibility;
            private set => SetProperty( ref _apiKeyVisibility, value );
        }

        public string APIKey
        {
            get => _apiKey;
            set => SetProperty( ref _apiKey, value );
        }

        public string EncryptedAPIKey
        {
            get => _encyptedApiKey;
            private set => SetProperty( ref _encyptedApiKey, value );
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

        public ObservableCollection<UnitTypes> UnitTypes { get; }

        public UnitTypes SelectedUnitType
        {
            get => _selectedUnitType;
            set => SetProperty( ref _selectedUnitType, value );
        }

        public double DistanceValue
        {
            get => _distanceValue;
            set => SetProperty( ref _distanceValue, value );
        }
    }
}
