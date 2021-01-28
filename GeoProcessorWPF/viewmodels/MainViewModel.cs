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
    public class MainViewModel : ObservableRecipient, IMainViewModel
    {
        private readonly IAppConfig _appConfig;
        private readonly IUserConfig _userConfig;
        private readonly IJ4JLogger? _logger;

        private bool _configIsValid;
        private ProcessWindow? _procWin;
        private bool _settingsChanged;

        public MainViewModel(
            IAppConfig appConfig,
            IUserConfig userConfig,
            IJ4JLogger? logger )
        {
            _appConfig = appConfig;
            _userConfig = userConfig;

            _logger = logger;
            _logger?.SetLoggedType( GetType() );

            SaveCommand = new RelayCommand( SaveCommandHandlerAsync );
            ProcessCommand = new RelayCommand( ProcessCommandHandlerAsync );

            IsActive = true;
        }

        protected override void OnActivated()
        {
            base.OnActivated();

            Messenger.Register<MainViewModel, FileConfigurationMessage, string>( this, 
                "primary",
                FileConfigurationMessageHandler );

            Messenger.Register<MainViewModel, ProcessingCompletedMessage, string>( this, 
                "primary",
                ProcessCompletedMessageHandler );

            Messenger.Register<MainViewModel, SettingsChangedMessage, string>(this,
                "primary",
                SettingsChangedHandler );
        }

        protected override void OnDeactivated()
        {
            base.OnDeactivated();

            Messenger.UnregisterAll(this);
        }

        private void FileConfigurationMessageHandler( MainViewModel recipient, FileConfigurationMessage fcMesg )
        {
            ConfigurationIsValid = fcMesg.ConfigurationIsValid;
        }

        private void ProcessCompletedMessageHandler( MainViewModel recipient, ProcessingCompletedMessage pcMesg )
        {
            _procWin?.Close();
            _procWin = null;
        }

        private void SettingsChangedHandler( MainViewModel recipient, SettingsChangedMessage scMesg )
        {
            SettingsChanged = true;
        }

        public bool ConfigurationIsValid
        {
            get => _configIsValid;
            private set => SetProperty( ref _configIsValid, value );
        }

        public bool SettingsChanged
        {
            get => _settingsChanged;
            private set => SetProperty( ref _settingsChanged, value );
        }

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

            _procWin = new ProcessWindow();
            _procWin.ShowDialog();
        }
    }
}
