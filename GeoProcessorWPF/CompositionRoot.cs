using System;
using System.IO;
using System.Windows;
using Autofac;
using J4JSoftware.DependencyInjection;
using J4JSoftware.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace J4JSoftware.GeoProcessor
{
    public class CompositionRoot : J4JCompositionRoot<J4JLoggerConfiguration>
    {
        public const string AppName = "GeoProcessorWPF";
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
            var provider = new ChannelConfigProvider( "Logging" )
                .AddChannel<ConsoleConfig>( "Channels:Console" )
                .AddChannel<DebugConfig>( "Channels:Debug" );

            if( InDesignMode )
            {
                var loggerConfig = new J4JLoggerConfiguration();
                loggerConfig.Channels.Add( new DebugConfig() );

                StaticConfiguredLogging( loggerConfig );
            }
            else ConfigurationBasedLogging( provider );

            UseConsoleLifetime = true;
        }

        public bool InDesignMode => System.ComponentModel.DesignerProperties
            .GetIsInDesignMode( new DependencyObject() );

        protected override void SetupConfigurationEnvironment( IConfigurationBuilder builder )
        {
            base.SetupConfigurationEnvironment( builder );

            builder.SetBasePath( Environment.CurrentDirectory )
                .AddJsonFile( Path.Combine( Environment.CurrentDirectory, AppConfigFile ), InDesignMode, false )
                .AddJsonFile( Path.Combine( AppUserFolder, UserConfigFile ), true, false )
                .AddUserSecrets<CompositionRoot>();
        }

        protected override void SetupDependencyInjection( HostBuilderContext hbc, ContainerBuilder builder )
        {
            base.SetupDependencyInjection( hbc, builder );

            if( InDesignMode )
                builder.RegisterType<AppConfigDesignTime>()
                    .AsImplementedInterfaces()
                    .SingleInstance();
            else
                builder.Register( c => hbc.Configuration.Get<AppConfig>() )
                    .AsImplementedInterfaces()
                    .SingleInstance();

            builder.RegisterType<MainViewModel>()
                .AsSelf()
                .SingleInstance();

            builder.RegisterType<MainWindow>()
                .AsSelf();

            builder.RegisterModule<AutofacGeoProcessorModule>();
        }
    }
}
