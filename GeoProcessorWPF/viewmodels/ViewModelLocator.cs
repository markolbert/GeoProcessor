using System.Windows;
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

        public MainViewModel MainViewModel => _host.Services.GetRequiredService<MainViewModel>();
    }
}