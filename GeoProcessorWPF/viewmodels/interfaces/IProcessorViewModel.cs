﻿using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace J4JSoftware.GeoProcessor
{
    public interface IProcessorViewModel : INotifyPropertyChanged, INotifyPropertyChanging
    {
        ProcessorState ProcessorState { get; }
        string Phase { get; set; }
        int PointsProcessed { get; }
        ObservableCollection<string> Messages { get; }
        Task OnWindowLoadedAsync();
        ICommand AbortCommand { get; }
    }
}