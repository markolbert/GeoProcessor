using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using J4JSoftware.Configuration.CommandLine;
using J4JSoftware.DependencyInjection;
using J4JSoftware.Logging;
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

        private static readonly J4JCachedLogger _cachedLogger = new J4JCachedLogger();
        private static CancellationToken _cancellationToken = new CancellationToken();

        private static async Task Main( string[] args )
        {
            Directory.CreateDirectory( AppUserFolder );
            
            var hostBuilder = InitializeHostBuilder();
            
            var host = hostBuilder.Build();

            // output any log events cached during configuration/startup
            var logger = host.Services.GetRequiredService<IJ4JLogger>();
            logger.OutputCache( _cachedLogger.Cache );

            await host.RunAsync( _cancellationToken );
        }

        private static IHostBuilder InitializeHostBuilder()
        {
            var retVal = new HostBuilder();
            retVal.UseConsoleLifetime();

            retVal.UseServiceProviderFactory( new AutofacServiceProviderFactory() );

            retVal.ConfigureHostConfiguration( builder =>
            {
                var options = new OptionCollection( CommandLineStyle.Linux, () => _cachedLogger);

                //options.SetTypePrefix<AppConfig>( "Configuration" );

                options.Bind<AppConfig, string?>(x => x.InputFile, "i", "inputFile");
                options.Bind<AppConfig, bool>(x => x.ZipOutputFile, "z", "zipOutput");
                options.Bind<AppConfig, double>(x => x.CoalesceValue, "d", "minDistanceValue");
                options.Bind<AppConfig, UnitTypes>(x => x.CoalesceUnit, "u", "minDistanceUnit");
                options.Bind<AppConfig, bool>(x => x.StoreAPIKey, "k", "storeApiKey");
                options.Bind<AppConfig, SnapProcessorType>(x => x.SnapProcessorType, "p", "snapProcessor");

                builder.SetBasePath( Environment.CurrentDirectory )
                    .AddUserSecrets<AppConfig>()
                    .AddJsonFile( Path.Combine( Environment.CurrentDirectory, "appConfig.json" ), false, false )
                    .AddJsonFile( AppUserConfigFile, true, false )
                    .AddJ4JCommandLine( Environment.CommandLine, options );
            } );

            retVal.ConfigureContainer<ContainerBuilder>( ( context, builder ) =>
            {
                //builder.Register( c =>
                //    {
                //        var temp = context.Configuration
                //                       .Get<AppConfig>();

                //        if( temp != null ) 
                //            return temp;

                //        _cachedLogger.Error("Failed to retrieve Configuration object from IConfiguration");
                //        temp = new AppConfig();

                //        return temp;
                //    } )
                //    .AsImplementedInterfaces()
                //    .SingleInstance();

                builder.RegisterType<KmlDocument>()
                    .AsSelf();

                builder.RegisterType<BingSnapRouteProcessor>()
                    .Keyed<ISnapRouteProcessor>( SnapProcessorType.Bing )
                    .AsImplementedInterfaces();

                var factory = new ChannelFactory( context.Configuration, "Logging", false );

                factory.AddChannel<ConsoleConfig>( "Logging:Channels:Console" );
                factory.AddChannel<DebugConfig>("Logging:Channels:Debug");

                builder.RegisterJ4JLogging<J4JLoggerConfiguration>( factory );
            } );

            retVal.ConfigureServices( ( context, services ) =>
            {
                services.AddOptions();
                services.Configure<AppConfig>( context.Configuration );

                AppConfig? config;

                try
                {
                    config = context.Configuration
                        .Get<AppConfig>();
                }
                catch( Exception e )
                {
                    _cachedLogger.Fatal<string>( 
                        "Failed to parse configuration information. Message was: {0}",
                        e.Message );

                    return;
                }

                if( config!.StoreAPIKey )
                    services.AddHostedService<StoreKeyApp>();
                else
                    services.AddHostedService<SnapApp>();
            } );

            return retVal;
        }
    }
}