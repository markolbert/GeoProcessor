using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using J4JSoftware.CommandLine;
using J4JSoftware.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace J4JSoftware.KMLProcessor
{
    internal class Program
    {
        public static string AppName = "GPS Track Processor";

        public static string AppUserFolder = Path.Combine( 
                Environment.GetFolderPath( Environment.SpecialFolder.LocalApplicationData ), 
                "J4JSoftware",
                AppName );

        public static string AppUserConfigFile = Path.Combine( AppUserFolder, "userConfig.json" );

        private static async Task Main( string[] args )
        {
            Directory.CreateDirectory( AppUserFolder );
            
            var hostBuilder = InitializeHostBuilder();

            await hostBuilder.RunConsoleAsync();
        }

        private static IHostBuilder InitializeHostBuilder()
        {
            var retVal = new HostBuilder();

            retVal.UseServiceProviderFactory( new AutofacServiceProviderFactory() );

            retVal.AddJ4JLogging<LoggingChannelConfig>();

            retVal.ConfigureHostConfiguration( builder =>
            {
                var options = new OptionCollection();

                options.SetTypePrefix<AppConfig>( "Configuration" );

                options.Bind<AppConfig, string?>(x => x.InputFile, "i", "inputFile");
                options.Bind<AppConfig, bool>(x => x.ZipOutputFile, "z", "zipOutput");
                options.Bind<AppConfig, double>(x => x.CoalesceValue, "d", "minDistanceValue");
                options.Bind<AppConfig, UnitTypes>(x => x.CoalesceUnit, "u", "minDistanceUnit");
                options.Bind<AppConfig, bool>(x => x.StoreAPIKey, "k", "storeApiKey");
                options.Bind<AppConfig, SnapProcessorType>(x => x.SnapProcessorType, "p", "snapProcessor");

                if( options.Log.HasMessages() )
                {
                    foreach( var logEntry in options.Log.GetMessages() )
                    {
                        Console.WriteLine( logEntry );
                    }
                }

                builder.SetBasePath( Environment.CurrentDirectory )
                    .AddUserSecrets<AppConfig>()
                    .AddJsonFile( Path.Combine( Environment.CurrentDirectory, "appConfig.json" ), false, false )
                    .AddJsonFile( AppUserConfigFile, true, false )
                    .AddJ4JCommandLine( Environment.CommandLine, options );
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
                AppConfig? config;

                try
                {
                    config = context.Configuration
                        .GetSection( "Configuration" )
                        .Get<AppConfig>();
                }
                catch( Exception e )
                {
                    Console.WriteLine("Failed to parse configuration information. Message was:");
                    Console.WriteLine(e.Message);

                    if( e.InnerException != null )
                        Console.WriteLine( e.InnerException.Message );

                    return;
                }

                if( config!.StoreAPIKey )
                    services.AddHostedService<StoreKeyApp>();
                else
                    services.AddHostedService<SnapApp>();
            } );

            return retVal;
        }

        private static OptionCollection ConfigureCommandLineOptions()
        {
            var retVal = new OptionCollection();

            retVal.Bind<AppConfig, string?>( x => x.InputFile, "i", "inputFile" );
            retVal.Bind<AppConfig, bool>(x => x.ZipOutputFile, "z", "zipOutput");
            retVal.Bind<AppConfig, double>( x => x.CoalesceValue, "d", "minDistanceValue" );
            retVal.Bind<AppConfig, UnitTypes>(x => x.CoalesceUnit, "u", "minDistanceUnit");
            retVal.Bind<AppConfig, bool>(x => x.StoreAPIKey, "k", "storeApiKey");
            retVal.Bind<AppConfig, SnapProcessorType>(x => x.SnapProcessorType, "p", "snapProcessor");

            return retVal;
        }
    }
}