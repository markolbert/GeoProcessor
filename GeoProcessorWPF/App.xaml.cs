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
        private async void Application_Startup( object sender, StartupEventArgs e )
        {
            var compRoot = TryFindResource( "ViewModelLocator" ) as CompositionRoot;
            if( compRoot?.Host == null )
                throw new NullReferenceException( "Couldn't find ViewModelLocator resource" );

            await compRoot.Host.StartAsync();

            var mainWindow = compRoot.Host.Services.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }

        private async void Application_Exit( object sender, ExitEventArgs e )
        {
            var compRoot = (CompositionRoot) TryFindResource( "ViewModelLocator" );

            using( compRoot.Host! )
            {
                await compRoot.Host!.StopAsync();
            }
        }
    }
}
