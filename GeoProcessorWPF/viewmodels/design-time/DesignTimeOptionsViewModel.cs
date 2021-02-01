using System.Windows.Input;
using Microsoft.Toolkit.Mvvm.ComponentModel;

#pragma warning disable 8618

namespace J4JSoftware.GeoProcessor
{
    public class DesignTimeOptionsViewModel : ObservableRecipient, IOptionsViewModel
    {
        private bool _settingsChanged;

        public DesignTimeOptionsViewModel( 
            IAppConfig appConfig,
            IUserConfig userConfig
            )
        {
        }

        #region Messaging

        protected override void OnActivated()
        {
            base.OnActivated();

            Messenger.Register<DesignTimeOptionsViewModel, SettingsChangedMessage, string>( this, 
                "primary",
                SettingsChangedMessageHandler );
        }

        private void SettingsChangedMessageHandler( DesignTimeOptionsViewModel recipient, SettingsChangedMessage scMesg )
        {
            SettingsChanged = true;
        }

        protected override void OnDeactivated()
        {
            base.OnDeactivated();

            Messenger.UnregisterAll( this );
        }

        #endregion

        public bool SettingsChanged
        {
            get => _settingsChanged;
            private set => SetProperty( ref _settingsChanged, value );
        }

        public ICommand SaveCommand { get; }
        public ICommand ReloadCommand { get; }
        public ICommand CloseCommand { get; }
    }
}
