using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace J4JSoftware.GeoProcessor
{
    public class ViewModelLocator
    {
        private readonly IHost _host;

        public ViewModelLocator()
        {
            _host = CompositionRoot.Default.Host!;
        }

        public IMainViewModel MainViewModel => _host.Services.GetRequiredService<IMainViewModel>();
        public IProcessFileViewModel ProcessFileViewModel => _host.Services.GetRequiredService<IProcessFileViewModel>();
        public IFileViewModel FileViewModel => _host.Services.GetRequiredService<IFileViewModel>();
        public IRouteOptionsViewModel RouteOptionsViewModel => _host.Services.GetRequiredService<IRouteOptionsViewModel>();
        public IProcessorViewModel ProcessorViewModel => _host.Services.GetRequiredService<IProcessorViewModel>();
    }
}