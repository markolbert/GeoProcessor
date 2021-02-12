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
using System.Windows;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace J4JSoftware.GeoProcessor
{
    public class DesignTimeRouteEnginesViewModel : ObservableRecipient, IRouteEnginesViewModel
    {
        private string _apiKey = string.Empty;
        private Visibility _apiKeyVisibility = Visibility.Collapsed;
        private double _distanceValue;
        private string _encyptedApiKey = string.Empty;
        private int _maxDistMultiplier;
        private ProcessorType _processorType = ProcessorType.None;
        private UnitTypes _selectedUnitType;

        public DesignTimeRouteEnginesViewModel()
        {
            ProcessorTypes.Add( ProcessorType.Bing );
            ProcessorTypes.Add( ProcessorType.Distance );
            ProcessorTypes.Add( ProcessorType.Google );

            SelectedProcessorType = ProcessorType.Google;

            APIKeyVisible = Visibility.Visible;
            APIKey = "...Google API key...";
            EncryptedAPIKey = "...should be encrypted...";

            MaxDistanceMultiplier = 3;

            UnitTypes = new ObservableCollection<UnitTypes>( Enum.GetValues<UnitTypes>() );
            SelectedUnitType = GeoProcessor.UnitTypes.mi;
            DistanceValue = 2.0;
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