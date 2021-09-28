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
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using J4JSoftware.Configuration.CommandLine;
using J4JSoftware.ConsoleUtilities;
using J4JSoftware.DependencyInjection;
using J4JSoftware.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace J4JSoftware.GeoProcessor
{
    internal class Program
    {
        public const string AppConfigFile = "appConfig.json";
        public const string UserConfigFile = "userConfig.json";

        internal static IHost? Host { get; private set; }

        private static readonly J4JCachedLogger _cachedLogger = new();
        private static readonly CancellationToken _cancellationToken = new();

        private static IJ4JLogger? _buildLogger;

        private static async Task Main( string[] args )
        {
            var hostConfig = new J4JHostConfiguration()
                .ApplicationName( "GeoProcessor" )
                .Publisher( "J4JSoftware" )
                .AddApplicationConfigurationFile( "appConfig.json" )
                .AddUserConfigurationFile( "userConfig.json" )
                .AddConfigurationInitializers( SetupConfiguration )
                .LoggerInitializer( SetupLogging )
                .FilePathTrimmer( FilePathTrimmer )
                .AddDependencyInjectionInitializers( SetupDependencyInjection )
                .AddServicesInitializers( SetupServices );

            hostConfig.AddCommandLineProcessing( CommandLineOperatingSystems.Windows )
                .OptionsInitializer( SetupOptions );

            _buildLogger = hostConfig.Logger;

            if( hostConfig.MissingRequirements != J4JHostRequirements.AllMet )
            {
                Console.WriteLine($"Could not create IHost. The following requirements were not met: {hostConfig.MissingRequirements.ToText()}");
                Environment.ExitCode = -1;

                return;
            }

            var builder = hostConfig.CreateHostBuilder();

            if( builder == null )
            {
                Console.WriteLine("Failed to create host builder.");
                Environment.ExitCode = -1;

                return;
            }

            Host = builder.Build();

            if ( Host == null )
            {
                Console.WriteLine( "Failed to build host" );
                Environment.ExitCode = -1;

                return;
            }

            await Host!.RunAsync( _cancellationToken );
        }

        private static void SetupConfiguration( IConfigurationBuilder builder )
        {
            builder.AddUserSecrets<AppConfig>();
        }

        private static void SetupLogging( IConfiguration config, J4JLoggerConfiguration loggerConfig )
            => loggerConfig.SerilogConfiguration.ReadFrom.Configuration( config );

        private static void SetupOptions( OptionCollection options )
        {
            options.Bind<AppConfig, string>(x => x.InputFile.FilePath, "i", "inputFile");
            options.Bind<AppConfig, string>(x => x.DefaultRouteName, "n", "defaultName");
            options.Bind<AppConfig, string>(x => x.OutputFile.FilePath, "o", "outputFile");
            options.Bind<AppConfig, ExportType>(x => x.ExportType, "t", "outputType");
            options.Bind<AppConfig, bool>(x => x.StoreAPIKey, "k", "storeApiKey");
            options.Bind<AppConfig, bool>(x => x.RunInteractive, "r", "runInteractive");
            options.Bind<AppConfig, ProcessorType>(x => x.ProcessorType, "p", "snapProcessor");
        }

        private static void SetupDependencyInjection( HostBuilderContext hbc, ContainerBuilder builder )
        {
            builder.Register( c =>
                {
                    AppConfig? config = null;

                    try
                    {
                        config = hbc.Configuration.Get<AppConfig>();

                        // decrypt the API keys
                        if( c.TryResolve<IJ4JProtection>( out var protection ) )
                        {
                            foreach( var apiKey in config.APIKeys )
                            {
                                if( protection.Unprotect( apiKey.Value.EncryptedValue, out var temp ) )
                                    apiKey.Value.Value = temp!;
                                else _buildLogger?.Error( "Could not decrypt API key for {0}", apiKey.Key );
                            }
                        }
                        else _buildLogger?.Error("Could not decrypt API keys");
                    }
                    catch( Exception e )
                    {
                        _buildLogger?.Fatal<string>(
                            "Failed to parse configuration information. Message was: {0}",
                            e.Message );
                    }

                    if( config == null )
                    {
                        config ??= new AppConfig();
                        _buildLogger?.Information("Created new instance of AppConfig");
                    }

                    if ( !string.IsNullOrEmpty( config.OutputFile.FileNameWithoutExtension ) )
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
                    x.Instance.Property( c => c.ProcessorType, new ProcessorTypeUpdater( _buildLogger ) );
                    x.Instance.Property( c => c.APIKey, new APIKeyUpdater( _buildLogger ) );
                } )
                .Named<IConfigurationUpdater>( StoreKeyApp.AutofacKey )
                .SingleInstance();

            builder.RegisterType<ConfigurationUpdater<AppConfig>>()
                .OnActivating( x =>
                {
                    x.Instance.Property( c => c.ProcessorType, new ProcessorTypeUpdater( _buildLogger ) );
                    x.Instance.Property( c => c.APIKey, new APIKeyUpdater( _buildLogger ) );
                    x.Instance.Property( c => c.InputFile, new InputFileUpdater( _buildLogger ) );
                } )
                .Named<IConfigurationUpdater>( RouteApp.AutofacKey )
                .SingleInstance();

            builder.RegisterModule<AutofacGeoProcessorModule>();
        }

        private static void SetupServices( HostBuilderContext hbc, IServiceCollection services )
        {
            var config = hbc.Configuration.Get<AppConfig>();

            if( config == null )
            {
                _buildLogger?.Fatal( "Cannot get AppConfig from IConfiguration" );

                return;
            }

            if( config.StoreAPIKey )
                services.AddHostedService<StoreKeyApp>();
            else
                services.AddHostedService<RouteApp>();
        }

        private static string FilePathTrimmer(
            Type? loggedType,
            string callerName,
            int lineNum,
            string srcFilePath)
        {
            return CallingContextEnricher.DefaultFilePathTrimmer(loggedType,
                callerName,
                lineNum,
                CallingContextEnricher.RemoveProjectPath(srcFilePath, GetProjectPath()));
        }

        private static string GetProjectPath([CallerFilePath] string filePath = "")
        {
            var dirInfo = new DirectoryInfo(Path.GetDirectoryName(filePath)!);

            while (dirInfo.Parent != null)
            {
                if (dirInfo.EnumerateFiles("*.csproj").Any())
                    break;

                dirInfo = dirInfo.Parent;
            }

            return dirInfo.FullName;
        }
    }
}