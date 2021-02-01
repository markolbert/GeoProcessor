using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;

namespace J4JSoftware.GeoProcessor
{
    public interface IRouteEnginesViewModel : INotifyPropertyChanged, INotifyPropertyChanging
    {
        ObservableCollection<ProcessorType> ProcessorTypes { get; }
        ProcessorType SelectedProcessorType { get; set; }
        Visibility APIKeyVisible { get; }
        string APIKey { get; set; }
        string EncryptedAPIKey { get; }
        int MaxDistanceMultiplier { get; set; }
        ObservableCollection<UnitTypes> UnitTypes { get; }
        UnitTypes SelectedUnitType { get; set; }
        double DistanceValue { get; set; }
    }
}