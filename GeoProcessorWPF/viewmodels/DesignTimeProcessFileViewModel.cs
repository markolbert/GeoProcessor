using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;

namespace J4JSoftware.GeoProcessor
{
    public class DesignTimeProcessFileViewModel : ObservableRecipient, IProcessFileViewModel
    {
        private int _pointProcessed;
        private string _phase = string.Empty;
        private string _cmdButtonText = string.Empty;
        private Visibility _cancelVisibility = Visibility.Visible;

        public DesignTimeProcessFileViewModel()
        {
            for( var idx = 0; idx < 5; idx++ )
            {
                Messages.Add( $"Message #{idx + 1}" );
            }

            ProcessCommand = new RelayCommand<ProcessWindow>( ProcessCommandAsync );

            CommandButtonText = "Start";
            Phase = "Ready to begin...";
            PointsProcessed = 55000;
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

        public bool Succeeded => true;
        
        public Visibility CancelVisibility
        {
            get => _cancelVisibility;
            private set => SetProperty( ref _cancelVisibility, value );
        }

        public ICommand CancelCommand {get;}

        public ICommand ProcessCommand { get; }

        private async void ProcessCommandAsync( ProcessWindow theWindow )
        {
        }
    }
}