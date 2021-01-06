using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using J4JSoftware.Configuration.CommandLine;
using J4JSoftware.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace J4JSoftware.KMLProcessor
{
    internal class Program
    {
        public const string AppName = "GPS Track Processor";
        public const string AppConfigFile = "appConfig.json";
        public const string UserConfigFile = "userConfig.json";

        public static string AppUserFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "J4JSoftware",
            AppName);

        private static readonly J4JCachedLogger _cachedLogger = new();
        private static readonly CancellationToken _cancellationToken = new();

        private static async Task Main( string[] args )
        {
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

                options.Bind<AppConfig, string?>(x => x.InputFile, "i", "inputFile");
                options.Bind<AppConfig, string>(x => x.DefaultRouteName, "n", "defaultName");
                options.Bind<AppConfig, string?>(x => x.OutputFile, "o", "outputFile");
                options.Bind<AppConfig, ExportType>(x => x.ExportType, "t", "outputType");
                options.Bind<AppConfig, bool>(x => x.StoreAPIKey, "k", "storeApiKey");
                options.Bind<AppConfig, ProcessorType>(x => x.ProcessorType, "p", "snapProcessor");

                builder.SetBasePath( Environment.CurrentDirectory )
                    .AddJsonFile( Path.Combine( Environment.CurrentDirectory, AppConfigFile ), false, false )
                    .AddJsonFile( Path.Combine( AppUserFolder, UserConfigFile ), true, false )
                    .AddUserSecrets<AppConfig>()
                    .AddJ4JCommandLine( options );
            } );

            retVal.ConfigureContainer<ContainerBuilder>( ( context, builder ) =>
            {
                builder.Register( c =>
                    {
                        AppConfig? config = null;

                        try
                        {
                            config = context.Configuration.Get<AppConfig>();
                        }
                        catch( Exception e )
                        {
                            _cachedLogger.Fatal<string>(
                                "Failed to parse configuration information. Message was: {0}",
                                e.Message);
                        }

                        config ??= new AppConfig();

                        // validate, but not when all we're doing is storing an API key
                        if( !config.StoreAPIKey )
                        {
                            var validator = c.Resolve<IAppConfigValidator>();
                            validator.Validate( config );
                        }

                        if ( !string.IsNullOrEmpty( config.OutputFileDetails.FileName ) ) 
                            return config;

                        config.OutputFileDetails.FilePath = config.InputFileDetails.FilePath;
                        config.OutputFileDetails.FileName = $"{config.OutputFileDetails.FileName}-processed";

                        config.OutputFileDetails.Type = config.InputFileDetails.Type switch
                        {
                            ImportType.KMZ => ExportType.KMZ,
                            _ => ExportType.KML
                        };

                        return config;
                    } )
                    .AsSelf()
                    .SingleInstance();

                builder.RegisterType<AppConfigValidator>()
                    .As<IAppConfigValidator>()
                    .SingleInstance();

                builder.RegisterType<KmlDocument>()
                    .AsSelf();

                builder.RegisterType<BingProcessor>()
                    .Keyed<IRouteProcessor>(KMLExtensions.GetTargetType<BingProcessor, RouteProcessorAttribute>()!.Type)
                    .AsImplementedInterfaces()
                    .SingleInstance();

                builder.RegisterType<GoogleProcessor>()
                    .Keyed<IRouteProcessor>(KMLExtensions.GetTargetType<GoogleProcessor, RouteProcessorAttribute>()!.Type)
                    .AsImplementedInterfaces()
                    .SingleInstance();

                builder.RegisterType<DistanceProcessor>()
                    .Keyed<IRouteProcessor>(KMLExtensions.GetTargetType<DistanceProcessor, RouteProcessorAttribute>()!.Type)
                    .AsImplementedInterfaces()
                    .SingleInstance();

                builder.RegisterType<KMLImporter>()
                    .Keyed<IImport>( KMLExtensions.GetTargetType<KMLImporter, ImporterAttribute>()!.Type )
                    .AsImplementedInterfaces()
                    .SingleInstance();

                builder.RegisterType<KMZImporter>()
                    .Keyed<IImport>(KMLExtensions.GetTargetType<KMZImporter, ImporterAttribute>()!.Type)
                    .AsImplementedInterfaces()
                    .SingleInstance();

                builder.RegisterType<GPXImporter>()
                    .Keyed<IImport>(KMLExtensions.GetTargetType<GPXImporter, ImporterAttribute>()!.Type)
                    .AsImplementedInterfaces()
                    .SingleInstance();

                builder.RegisterType<KMLExporter>()
                    .Keyed<IExport>(KMLExtensions.GetTargetType<KMLExporter, ExporterAttribute>()!.Type)
                    .AsImplementedInterfaces()
                    .SingleInstance();

                builder.RegisterType<KMZExporter>()
                    .Keyed<IExport>(KMLExtensions.GetTargetType<KMZExporter, ExporterAttribute>()!.Type)
                    .AsImplementedInterfaces()
                    .SingleInstance();

                var factory = new ChannelFactory( context.Configuration, "Logging", false );

                factory.AddChannel<ConsoleConfig>( "Logging:Channels:Console" );
                factory.AddChannel<DebugConfig>("Logging:Channels:Debug");

                builder.RegisterJ4JLogging<J4JLoggerConfiguration>( factory );
            } );

            retVal.ConfigureServices( ( context, services ) =>
            {
                var config = context.Configuration.Get<AppConfig>();

                if ( config!.StoreAPIKey )
                    services.AddHostedService<StoreKeyApp>();
                else
                    services.AddHostedService<RouteApp>();
            } );

            return retVal;
        }
    }
}