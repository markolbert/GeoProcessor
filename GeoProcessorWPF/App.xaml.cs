using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace J4JSoftware.GeoProcessor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly IHost _host;

        public App()
        {
            _host = CompositionRoot.Default.Host!;
        }

        private async void Application_Startup( object sender, StartupEventArgs e )
        {
            await _host.StartAsync();

            var mainWindow = _host.Services.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }

        private async void Application_Exit( object sender, ExitEventArgs e )
        {
            using( _host )
            {
                await _host.StopAsync();
            }
        }
    }
}
