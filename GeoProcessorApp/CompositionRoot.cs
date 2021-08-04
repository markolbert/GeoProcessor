#region license

// Copyright 2021 Mark A. Olbert
// 
// This library or program 'GeoProcessorApp' is free software: you can redistribute it
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
using System.Drawing;
using System.IO;
using System.Linq;
using Autofac;
using J4JSoftware.Configuration.CommandLine;
using J4JSoftware.ConsoleUtilities;
using J4JSoftware.DependencyInjection;
using J4JSoftware.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace J4JSoftware.GeoProcessor
{
    public class CompositionRoot : ConsoleRoot<AppConfig, LoggerConfigurator>
    {
        private static CompositionRoot? _compRoot;

        public static CompositionRoot Default
        {
            get
            {
                if( _compRoot != null ) 
                    return _compRoot;

                _compRoot = new CompositionRoot();
                _compRoot.Build();

                return _compRoot;
            }
        }

        private CompositionRoot()
            : base(
                "J4JSoftware",
                Program.AppName,
                dataProtectionPurpose: "J4JSoftware.GeoProcessor.DataProtection"
            )
        {
        }

        protected override void RegisterLoggerConfiguration( ContainerBuilder builder )
        {
            // no op, because we've already registered AppConfig for other reasons
        }

        protected override void ConfigureLogger( J4JLogger logger ) =>
            LoggerConfigurator.Configure( logger, LoggerConfig!.Logging );

        protected override void SetupConfigurationEnvironment( IConfigurationBuilder builder )
        {
            base.SetupConfigurationEnvironment( builder );

            var options = new OptionCollection( CommandLineStyle.Linux, loggerFactory: () => CachedLogger );

            options.Bind<AppConfig, string>( x => x.InputFile.FilePath, "i", "inputFile" );
            options.Bind<AppConfig, string>( x => x.DefaultRouteName, "n", "defaultName" );
            options.Bind<AppConfig, string>( x => x.OutputFile.FilePath, "o", "outputFile" );
            options.Bind<AppConfig, ExportType>( x => x.ExportType, "t", "outputType" );
            options.Bind<AppConfig, bool>( x => x.StoreAPIKey, "k", "storeApiKey" );
            options.Bind<AppConfig, bool>( x => x.RunInteractive, "r", "runInteractive" );
            options.Bind<AppConfig, ProcessorType>( x => x.ProcessorType, "p", "snapProcessor" );

            builder.SetBasePath( Environment.CurrentDirectory )
                .AddJsonFile( Path.Combine( Environment.CurrentDirectory, Program.AppConfigFile ), false, false )
                .AddJsonFile( Path.Combine( Program.AppUserFolder, Program.UserConfigFile ), true, false )
                .AddUserSecrets<AppConfig>()
                .AddJ4JCommandLine( options );
        }

        protected override void SetupDependencyInjection( HostBuilderContext hbc, ContainerBuilder builder )
        {
            base.SetupDependencyInjection( hbc, builder );

            // register AppConfig
            builder.Register( c =>
                {
                    AppConfig? config = null;

                    try
                    {
                        config = hbc.Configuration.Get<AppConfig>();
                    }
                    catch( Exception e )
                    {
                        CachedLogger.Fatal<string>(
                            "Failed to parse configuration information. Message was: {0}",
                            e.Message );
                    }

                    config ??= new AppConfig();

                    if( !string.IsNullOrEmpty( config.OutputFile.FileNameWithoutExtension ) )
                        return config;

                    config.OutputFile.FilePath = config.InputFile.FilePath;
                    config.OutputFile.FileNameWithoutExtension =
                        $"{config.OutputFile.FileNameWithoutExtension}-processed";

                    config.OutputFile.Type = config.InputFile.Type switch
                    {
                        ImportType.KMZ => ExportType.KMZ,
                        _ => ExportType.KML
                    };

                    return config;
                } )
                .AsImplementedInterfaces()
                .AsSelf()
                .SingleInstance();

            builder.RegisterType<ConfigurationUpdater<AppConfig>>()
                .OnActivating( x =>
                {
                    x.Instance.Property( c => c.ProcessorType, new ProcessorTypeUpdater( CachedLogger ) );
                    x.Instance.Property( c => c.APIKey, new APIKeyUpdater( CachedLogger ) );
                } )
                .Named<IConfigurationUpdater>( StoreKeyApp.AutofacKey )
                .SingleInstance();

            builder.RegisterType<ConfigurationUpdater<AppConfig>>()
                .OnActivating( x =>
                {
                    x.Instance.Property( c => c.ProcessorType, new ProcessorTypeUpdater( CachedLogger ) );
                    x.Instance.Property( c => c.APIKey, new APIKeyUpdater( CachedLogger ) );
                    x.Instance.Property( c => c.InputFile, new InputFileUpdater( CachedLogger ) );
                } )
                .Named<IConfigurationUpdater>( RouteApp.AutofacKey )
                .SingleInstance();

            builder.RegisterModule<AutofacGeoProcessorModule>();
        }

        protected override void SetupServices( HostBuilderContext hbc, IServiceCollection services )
        {
            base.SetupServices( hbc, services );

            services.AddDataProtection();

            var config = hbc.Configuration.Get<AppConfig>();

            if( config!.StoreAPIKey )
                services.AddHostedService<StoreKeyApp>();
            else
                services.AddHostedService<RouteApp>();
        }
    }
}