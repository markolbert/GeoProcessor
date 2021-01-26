using J4JSoftware.Logging;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace J4JSoftware.GeoProcessor
{
    public class MainViewModel : ObservableRecipient
    {
        private readonly IAppConfig _appConfig;
        private readonly IJ4JLogger? _logger;

        public MainViewModel(
            FileViewModel fileVM,
            RouteOptionsViewModel routeOptionsVM,
            ProcessorViewModel procVM,
            IAppConfig appConfig,
            IJ4JLogger? logger )
        {
            _appConfig = appConfig;

            _logger = logger;
            _logger?.SetLoggedType( GetType() );

            FileViewModel = fileVM;
            RouteOptionsViewModel = routeOptionsVM;
            ProcessorViewModel = procVM;
        }

        public FileViewModel FileViewModel { get; }
        public RouteOptionsViewModel RouteOptionsViewModel { get; }
        public ProcessorViewModel ProcessorViewModel { get; }
    }
}
