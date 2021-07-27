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
using J4JSoftware.WPFViewModel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace J4JSoftware.GeoProcessor
{
    public class CompositionRoot : XamlJ4JCompositionRoot<J4JLoggerConfiguration>
    {
        public const string AppName = "GeoProcessor";
        public const string AppConfigFile = "appConfig.json";
        public const string UserConfigFile = "userConfig.json";

        public CompositionRoot()
            : base( 
                "J4JSoftware", 
                AppName, 
                ()=>DesignerProperties.GetIsInDesignMode(new DependencyObject()),
                "J4JSoftware.GeoProcessor.DataProtection" )
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
        public IProcessorViewModel ProcessorViewModel => Host!.Services.GetRequiredService<IProcessorViewModel>();
        public IOptionsViewModel OptionsViewModel => Host!.Services.GetRequiredService<IOptionsViewModel>();

        public IRouteDisplayViewModel RouteDisplayViewModel =>
            Host!.Services.GetRequiredService<IRouteDisplayViewModel>();

        public IRouteEnginesViewModel RouteEnginesViewModel =>
            Host!.Services.GetRequiredService<IRouteEnginesViewModel>();

        protected override void SetupConfigurationEnvironment( IConfigurationBuilder builder )
        {
            base.SetupConfigurationEnvironment( builder );

            builder.SetBasePath( Environment.CurrentDirectory )
                .AddJsonFile( Path.Combine( ApplicationConfigurationFolder, AppConfigFile ), false, false )
                .AddJsonFile( Path.Combine( UserConfigurationFolder, UserConfigFile ), true, false )
                .AddUserSecrets<CompositionRoot>();
        }

        protected override void RegisterViewModels( ViewModelDependencyBuilder builder )
        {
            base.RegisterViewModels( builder );

            builder.RegisterViewModelInterface<IMainViewModel>()
                .DesignTime<DesignTimeMainViewModel>()
                .RunTime<MainViewModel>();

            builder.RegisterViewModelInterface<IOptionsViewModel>()
                .DesignTime<DesignTimeOptionsViewModel>()
                .RunTime<OptionsViewModel>();

            builder.RegisterViewModelInterface<IRouteDisplayViewModel>()
                .DesignTime<DesignTimeRouteDisplayViewModel>()
                .RunTime<RouteDisplayViewModel>();

            builder.RegisterViewModelInterface<IRouteEnginesViewModel>()
                .DesignTime<DesignTimeRouteEnginesViewModel>()
                .RunTime<RouteEnginesViewModel>();

            builder.RegisterViewModelInterface<IProcessorViewModel>()
                .DesignTime<DesignTimeProcessorViewModel>()
                .RunTime<ProcessorViewModel>();
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

                    foreach( var kvp in retVal.APIKeys ) kvp.Value.Initialize( protection );

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