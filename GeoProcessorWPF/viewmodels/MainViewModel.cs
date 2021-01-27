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
    public class MainViewModel : ObservableRecipient
    {
        private readonly IAppConfig _appConfig;
        private readonly IUserConfig _userConfig;
        private readonly IJ4JLogger? _logger;

        private bool _configIsValid;

        public MainViewModel(
            FileViewModel fileVM,
            RouteOptionsViewModel routeOptionsVM,
            ProcessorViewModel procVM,
            IAppConfig appConfig,
            IUserConfig userConfig,
            IJ4JLogger? logger )
        {
            _appConfig = appConfig;
            _userConfig = userConfig;

            _logger = logger;
            _logger?.SetLoggedType( GetType() );

            FileViewModel = fileVM;
            FileViewModel.PropertyChanged += ( fvm, args ) => CheckIfValid();
            CheckIfValid();

            RouteOptionsViewModel = routeOptionsVM;
            ProcessorViewModel = procVM;

            SaveCommand = new RelayCommand( SaveCommandHandlerAsync );
            ProcessCommand = new RelayCommand( ProcessCommandHandlerAsync );
        }

        public FileViewModel FileViewModel { get; }
        public RouteOptionsViewModel RouteOptionsViewModel { get; }
        public ProcessorViewModel ProcessorViewModel { get; }

        public bool ConfigurationIsValid
        {
            get => _configIsValid;
            private set => SetProperty( ref _configIsValid, value );
        }

        private void CheckIfValid() =>
            ConfigurationIsValid = File.Exists( FileViewModel.InputPath )
                                   && FileViewModel.SelectedSnappingType != ProcessorType.Undefined
                                   && !string.IsNullOrEmpty( FileViewModel.OutputPath );

        public ICommand SaveCommand { get; }

        private async void SaveCommandHandlerAsync()
        {
            var options = new JsonSerializerOptions { WriteIndented = true };

            var userText = JsonSerializer.Serialize( _userConfig, options );

            await File.WriteAllTextAsync(
                Path.Combine( CompositionRoot.Default.UserConfigurationFolder, CompositionRoot.UserConfigFile ),
                userText );

            var appText = JsonSerializer.Serialize( _appConfig, options );

            await File.WriteAllTextAsync(
                Path.Combine( CompositionRoot.Default.ApplicationConfigurationFolder, CompositionRoot.AppConfigFile ),
                appText );
        }

        public ICommand ProcessCommand { get; }

        private async void ProcessCommandHandlerAsync()
        {
            if( File.Exists( _appConfig.OutputFile.FilePath ) )
            {
                var mainWin = Application.Current.MainWindow as MetroWindow;

                var result = await mainWin!.ShowMessageAsync( "Output File Exists",
                    "The output file exists. Do you want to overwrite it?", 
                    MessageDialogStyle.AffirmativeAndNegative );

                if( result == MessageDialogResult.Negative )
                    return;
            }

            var procWin = new ProcessWindow();

            procWin.ShowDialog();
        }
    }
}
