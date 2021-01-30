using System;
using System.IO;
using Autofac;
using J4JSoftware.DependencyInjection;
using J4JSoftware.Logging;
using J4JSoftware.WPFViewModel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace J4JSoftware.GeoProcessor
{
    public class CompositionRoot : J4JViewModelLocator<J4JLoggerConfiguration>
    {
        public const string AppName = "GeoProcessor";
        public const string AppConfigFile = "appConfig.json";
        public const string UserConfigFile = "userConfig.json";

        public CompositionRoot()
            : base( "J4JSoftware", AppName, "J4JSoftware.GeoProcessor.DataProtection" )
        {
            NetEventChannelConfiguration = new NetEventConfig
            {
                OutputTemplate = "[{Level:u3}] {Message}",
                EventElements = EventElements.None
            };

            if( InDesignMode )
            {
                var loggerConfig = new J4JLoggerConfiguration();

                loggerConfig.Channels.Add( new DebugConfig() );
                loggerConfig.Channels.Add( NetEventChannelConfiguration );

                StaticConfiguredLogging( loggerConfig );
            }
            else
            {
                var provider = new ChannelConfigProvider( "Logging" )
                    .AddChannel<ConsoleConfig>( "Channels:Console" )
                    .AddChannel<DebugConfig>( "Channels:Debug" );

                provider.AddChannel( NetEventChannelConfiguration );

                ConfigurationBasedLogging( provider );
            }

            Initialize();
        }

        public NetEventConfig NetEventChannelConfiguration { get; }
        public IMainViewModel MainViewModel => Host!.Services.GetRequiredService<IMainViewModel>();
        public IProcessFileViewModel ProcessFileViewModel => Host!.Services.GetRequiredService<IProcessFileViewModel>();
        public IRouteDisplayViewModel RouteDisplayViewModel => Host!.Services.GetRequiredService<IRouteDisplayViewModel>();
        public IProcessorViewModel ProcessorViewModel => Host!.Services.GetRequiredService<IProcessorViewModel>();

        protected override void SetupConfigurationEnvironment( IConfigurationBuilder builder )
        {
            base.SetupConfigurationEnvironment( builder );

            builder.SetBasePath( Environment.CurrentDirectory )
                .AddJsonFile( Path.Combine( ApplicationConfigurationFolder, AppConfigFile ), false,false )
                .AddJsonFile( Path.Combine( UserConfigurationFolder, UserConfigFile ), true, false )
                .AddUserSecrets<CompositionRoot>();
        }

        protected override void RegisterViewModels( ViewModelDependencyBuilder builder )
        {
            base.RegisterViewModels( builder );

            builder.RegisterViewModelInterface<IMainViewModel>()
                .DesignTime<DesignTimeMainViewModel>()
                .RunTime<MainViewModel>();

            builder.RegisterViewModelInterface<IRouteDisplayViewModel>()
                .DesignTime<DesignTimeRouteDisplayViewModel>()
                .RunTime<RouteDisplayViewModel>();

            builder.RegisterViewModelInterface<IProcessorViewModel>()
                .DesignTime<DesignTimeProcessorViewModel>()
                .RunTime<ProcessorViewModel>();

            builder.RegisterViewModelInterface<IProcessFileViewModel>()
                .DesignTime<DesignTimeProcessFileViewModel>()
                .RunTime<ProcessFileViewModel>();
        }

        protected override void SetupDependencyInjection( HostBuilderContext hbc, ContainerBuilder builder )
        {
            base.SetupDependencyInjection( hbc, builder );

            builder.Register( c =>
                {
                    var retVal = hbc.Configuration.Get<AppConfig>();

                    retVal.ApplicationConfigurationFolder = ApplicationConfigurationFolder;
                    retVal.UserConfigurationFolder = UserConfigurationFolder;
                    retVal.NetEventChannelConfiguration = NetEventChannelConfiguration;

                    return retVal;
                } )
                .AsImplementedInterfaces()
                .SingleInstance();

            builder.Register( c =>
                {
                    var retVal = hbc.Configuration.Get<UserConfig>();

                    var protection = c.Resolve<IJ4JProtection>();

                    foreach( var kvp in retVal.APIKeys )
                    {
                        kvp.Value.Initialize( protection );
                    }

                    return retVal;
                } )
                .AsImplementedInterfaces()
                .SingleInstance();

            builder.RegisterType<MainWindow>()
                .AsSelf();

            builder.RegisterModule<AutofacGeoProcessorModule>();
        }
    }
}
