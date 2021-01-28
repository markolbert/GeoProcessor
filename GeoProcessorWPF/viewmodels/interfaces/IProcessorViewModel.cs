using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;

namespace J4JSoftware.GeoProcessor
{
    public interface IProcessorViewModel : INotifyPropertyChanged, INotifyPropertyChanging
    {
        ObservableCollection<ProcessorType> ProcessorTypes { get; }
        ProcessorType SelectedProcessorType { get; set; }
        Visibility APIKeyVisible { get; }
        string APIKey { get; set; }
        string EncryptedAPIKey { get; }
        int MaxDistanceMultiplier { get; set; }
        Visibility RequestLimitVisibility { get; }
        int MaxPointsPerRequest { get; set; }
        ObservableCollection<UnitTypes> UnitTypes { get; }
        UnitTypes SelectedUnitType { get; set; }
        double DistanceValue { get; set; }
    }
}