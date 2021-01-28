using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using J4JSoftware.Logging;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;

namespace J4JSoftware.GeoProcessor
{
    public class DesignTimeMainViewModel : ObservableRecipient, IMainViewModel
    {
        private bool _configIsValid;

        public DesignTimeMainViewModel(
            IFileViewModel fileVM,
            IRouteOptionsViewModel routeOptionsVM,
            IProcessorViewModel procVM )
        {
            FileViewModel = fileVM;
            RouteOptionsViewModel = routeOptionsVM;
            ProcessorViewModel = procVM;
            ConfigurationIsValid = true;
        }

        public IFileViewModel FileViewModel { get; }
        public IRouteOptionsViewModel RouteOptionsViewModel { get; }
        public IProcessorViewModel ProcessorViewModel { get; }

        public bool ConfigurationIsValid
        {
            get => _configIsValid;
            private set => SetProperty( ref _configIsValid, value );
        }

        public ICommand SaveCommand { get; }
        public ICommand ProcessCommand { get; }
    }
}
