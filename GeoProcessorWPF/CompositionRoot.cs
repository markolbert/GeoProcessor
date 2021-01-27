using System;
using System.IO;
using System.Windows;
using Autofac;
using J4JSoftware.DependencyInjection;
using J4JSoftware.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace J4JSoftware.GeoProcessor
{
    public class CompositionRoot : J4JCompositionRoot<J4JLoggerConfiguration>
    {
        public const string AppName = "GeoProcessor";
        public const string AppConfigFile = "appConfig.json";
        public const string UserConfigFile = "userConfig.json";

        public static string AppUserFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "J4JSoftware",
            AppName);

        public static CompositionRoot Default { get; }

        static CompositionRoot()
        {
            Default = new CompositionRoot();

            Default.Initialize();
        }

        private CompositionRoot()
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

            UseConsoleLifetime = true;
        }

        public bool InDesignMode => System.ComponentModel.DesignerProperties
            .GetIsInDesignMode( new DependencyObject() );

        public NetEventConfig NetEventChannelConfiguration { get; }

        protected override void SetupConfigurationEnvironment( IConfigurationBuilder builder )
        {
            base.SetupConfigurationEnvironment( builder );

            var appDir = InDesignMode ? AppContext.BaseDirectory : Environment.CurrentDirectory;

            builder.SetBasePath( Environment.CurrentDirectory )
                .AddJsonFile( Path.Combine( appDir, AppConfigFile ), false,false )
                .AddJsonFile( Path.Combine( AppUserFolder, UserConfigFile ), true, false )
                .AddUserSecrets<CompositionRoot>();
        }

        protected override void SetupDependencyInjection( HostBuilderContext hbc, ContainerBuilder builder )
        {
            base.SetupDependencyInjection( hbc, builder );

            builder.Register( c => hbc.Configuration.Get<AppConfig>() )
                .AsImplementedInterfaces()
                .SingleInstance();

            builder.Register( c => hbc.Configuration.Get<UserConfig>() )
                .AsImplementedInterfaces()
                .SingleInstance();

            builder.RegisterType<MainViewModel>()
                .AsSelf();

            builder.RegisterType<FileViewModel>()
                .AsSelf();

            builder.RegisterType<RouteOptionsViewModel>()
                .AsSelf();
            
            builder.RegisterType<ProcessorViewModel>()
                .AsSelf();

            if( InDesignMode )
                builder.RegisterType<DesignTimeProcessFileViewModel>()
                    .AsImplementedInterfaces();
            else
                builder.RegisterType<ProcessFileViewModel>()
                    .AsImplementedInterfaces();
            
            builder.RegisterType<MainWindow>()
                .AsSelf();

            builder.RegisterModule<AutofacGeoProcessorModule>();
        }
    }
}
