#region license

// Copyright 2021 Mark A. Olbert
// 
// This library or program 'GeoProcessorWPF' is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public License as
// published by the Free Software Foundation, either version 3 of the License,
// or (at your option) any later version.
// 
// This library or program is distributed in the hope that it will be useful, but
// WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// General Public License for more details.
// 
// You should have received a copy of the GNU General Public License along with
// this library or program.  If not, see <https://www.gnu.org/licenses/>.

#endregion

using System;
using System.ComponentModel;
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
    public class CompositionRoot : XamlJ4JCompositionRoot
    {
        public const string AppName = "GeoProcessor";
        public const string AppConfigFile = "appConfig.json";
        public const string UserConfigFile = "userConfig.json";

        public static CompositionRoot Default { get; } = new();

        public CompositionRoot()
            : base( 
                "J4JSoftware", 
                AppName, 
                ()=>DesignerProperties.GetIsInDesignMode(new DependencyObject()),
                "J4JSoftware.GeoProcessor.DataProtection" )
        {
            Initialize();
        }

        protected override void ConfigureLoggerDefaults( J4JLogger logger, IConfiguration configuration )
        {
            var loggerInfo = new LoggerInfo( configuration );

            logger.ApplySettings( loggerInfo );

            if( loggerInfo.ChannelSpecific == null )
            {
                CachedLogger.Error("No logging channels defined");
                return;
            }

            foreach( var kvp in loggerInfo.ChannelSpecific )
            {
                switch( kvp.Key.ToLower() )
                {
                    case "debug":
                        var debugChannel = logger.AddDebug();
                        debugChannel.Parameters.ApplySettings( kvp.Value );
                        break;

                    case "console":
                        var consoleChannel = logger.AddConsole();
                        consoleChannel.Parameters.ApplySettings(kvp.Value);
                        break;

                    case "file":
                        var fileChannel = logger.AddFile();
                        fileChannel.Parameters.ApplySettings(kvp.Value);
                        break;

                    default:
                        CachedLogger.Error<string>( "Unsupported J4JLogging channel '{0}'", kvp.Key );
                        break;
                }
            }

            NetEventChannel = logger.AddNetEvent();
        }

        public NetEventChannel? NetEventChannel { get; private set; }

        public MainVM MainVM => Host!.Services.GetRequiredService<MainVM>();
        public ProcessorVM ProcessorVM => Host!.Services.GetRequiredService<ProcessorVM>();
        public OptionsVM OptionsVM => Host!.Services.GetRequiredService<OptionsVM>();
        public RouteDisplayVM RouteDisplayVM => Host!.Services.GetRequiredService<RouteDisplayVM>();
        public RouteEnginesVM RouteEnginesVM => Host!.Services.GetRequiredService<RouteEnginesVM>();

        protected override void SetupConfigurationEnvironment( IConfigurationBuilder builder )
        {
            base.SetupConfigurationEnvironment( builder );

            builder.SetBasePath( Environment.CurrentDirectory )
                .AddJsonFile( Path.Combine( ApplicationConfigurationFolder, AppConfigFile ), false, false )
                .AddJsonFile( Path.Combine( UserConfigurationFolder, UserConfigFile ), true, false )
                .AddUserSecrets<CompositionRoot>();
        }

        protected override void SetupDependencyInjection( HostBuilderContext hbc, ContainerBuilder builder )
        {
            base.SetupDependencyInjection( hbc, builder );

            builder.Register( c =>
                {
                    var retVal = hbc.Configuration.Get<AppConfig>();

                    retVal.ApplicationConfigurationFolder = ApplicationConfigurationFolder;
                    retVal.UserConfigurationFolder = UserConfigurationFolder;
                    retVal.NetEventChannel = NetEventChannel;

                    return retVal;
                } )
                .AsImplementedInterfaces()
                .SingleInstance();

            builder.Register( c =>
                {
                    var retVal = hbc.Configuration.Get<UserConfig>();

                    var protection = c.Resolve<IJ4JProtection>();

                    foreach( var kvp in retVal.APIKeys ) kvp.Value.Initialize( protection );

                    return retVal;
                } )
                .AsImplementedInterfaces()
                .SingleInstance();

            builder.RegisterType<MainWindow>()
                .AsSelf();

            builder.RegisterType<MainVM>()
                .AsSelf();

            builder.RegisterType<OptionsVM>()
                .AsSelf();

            builder.RegisterType<RouteDisplayVM>()
                .AsSelf();

            builder.RegisterType<RouteEnginesVM>()
                .AsSelf();

            builder.RegisterType<ProcessorVM>()
                .AsSelf();

            builder.RegisterModule<AutofacGeoProcessorModule>();
        }
    }
}