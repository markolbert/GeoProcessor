using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Autofac;
using J4JSoftware.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace J4JSoftware.KMLProcessor
{
    internal class Program
    {
        public static string AppName = "GPS Track Processor";

        private static async Task Main( string[] args )
        {
            var hostBuilder = InitializeHostBuilder();

            await hostBuilder.RunConsoleAsync();
        }

        private static IHostBuilder InitializeHostBuilder()
        {
            var retVal = new J4JHostBuilder();

            retVal.AddJ4JLogging<LoggingChannelConfig>();

            retVal.ConfigureHostConfiguration( builder =>
            {
                builder
                    .SetBasePath( Environment.CurrentDirectory )
                    .AddUserSecrets<AppConfig>()
                    .AddJsonFile( Path.Combine( Environment.CurrentDirectory, "appConfig.json" ), false, false )
                    .AddJsonFile( Path.Combine(
                            Environment.GetFolderPath( Environment.SpecialFolder.LocalApplicationData ),
                            AppName,
                            "userConfig.json" ),
                        true, false )
                    .AddCommandLine( J4JHostBuilder.ExpandUnvaluedCommandLineSwitches(),
                        AppConfig.CommandLineMappings );
            } );

            retVal.ConfigureContainer<ContainerBuilder>( ( context, builder ) =>
            {
                builder.Register( c =>
                    {
                        var temp = context.Configuration
                            .GetSection( "Configuration" )
                            .Get<AppConfig>();

                        return temp ?? new AppConfig();
                    } )
                    .AsImplementedInterfaces()
                    .SingleInstance();

                builder.RegisterType<KmlDocument>()
                    .AsSelf();

                builder.RegisterType<BingSnapRouteProcessor>()
                    .Keyed<ISnapRouteProcessor>( SnapProcessorType.Bing )
                    .AsImplementedInterfaces();
            } );

            retVal.ConfigureServices( ( context, services ) =>
            {
                var config = context.Configuration
                    .GetSection( "Configuration" )
                    .Get<AppConfig>();

                if( config.StoreAPIKey )
                    services.AddHostedService<StoreKeyApp>();
                else
                    services.AddHostedService<SnapApp>();
            } );

            return retVal;
        }
    }
}