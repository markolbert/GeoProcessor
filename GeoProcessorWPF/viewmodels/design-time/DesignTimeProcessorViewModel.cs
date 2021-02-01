using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
// ReSharper disable CollectionNeverUpdated.Local
// ReSharper disable UnassignedGetOnlyAutoProperty
#pragma warning disable 1998
#pragma warning disable 8618
#pragma warning disable 649

namespace J4JSoftware.GeoProcessor
{
    public class DesignTimeProcessorViewModel : ObservableRecipient, IProcessorViewModel
    {
        private int _pointProcessed;
        private string _phase = string.Empty;
        private string _cmdButtonText = string.Empty;

        public DesignTimeProcessorViewModel()
        {
            for( var idx = 0; idx < 5; idx++ )
            {
                Messages.Add( $"Message #{idx + 1}" );
            }

            CommandButtonText = "Start";
            Phase = "Ready to begin...";
            PointsProcessed = 55000;
        }

        public ProcessorState ProcessorState { get; }

        public async Task OnWindowLoadedAsync()
        {
        }

        public string Phase
        {
            get => _phase;
            set => SetProperty( ref _phase, value );
        }

        public int PointsProcessed
        {
            get => _pointProcessed;
            private set => SetProperty( ref _pointProcessed, value );
        }

        public ObservableCollection<string> Messages { get; } = new();

        public string CommandButtonText
        {
            get => _cmdButtonText;
            private set => SetProperty( ref _cmdButtonText, value );
        }

        public ICommand WindowLoaded { get; }
        public ICommand AbortCommand {get;}

        public async Task ProcessAsync()
        {
            throw new NotImplementedException();
        }
    }
}