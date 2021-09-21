using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using J4JSoftware.Configuration.CommandLine;
using J4JSoftware.DependencyInjection;
using J4JSoftware.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace J4JSoftware.GeoProcessor
{
    public class ViewModelLocator
    {
        public const string AppConfigFile = "appConfig.json";
        public const string UserConfigFile = "userConfig.json";

        private readonly IHost _host;

        private IJ4JLogger? _buildLogger;

        public ViewModelLocator()
        {
            _host = CreateHost();
        }

        public MainVM MainVM => _host!.Services.GetRequiredService<MainVM>();
        public ProcessorVM ProcessorVM => _host!.Services.GetRequiredService<ProcessorVM>();
        public OptionsVM OptionsVM => _host!.Services.GetRequiredService<OptionsVM>();
        public RouteDisplayVM RouteDisplayVM => _host!.Services.GetRequiredService<RouteDisplayVM>();
        public RouteEnginesVM RouteEnginesVM => _host!.Services.GetRequiredService<RouteEnginesVM>();

        private IHost CreateHost()
        {
            var hostConfig = new J4JHostConfiguration()
                .Publisher( "J4JSoftware" )
                .ApplicationName( "GeoProcessor" )
                .AddApplicationConfigurationFile( AppConfigFile )
                .AddUserConfigurationFile( UserConfigFile )
                .AddConfigurationInitializers( ConfigurationInitializer )
                .LoggerInitializer( LoggerInitializer )
                .AddNetEventSinkToLogger()
                .FilePathTrimmer( FilePathTrimmer )
                .AddDependencyInjectionInitializers( SetupDependencyInjection );

            _buildLogger = hostConfig.Logger;

            if( hostConfig.MissingRequirements != J4JHostRequirements.AllMet )
                throw new ArgumentException(
                    $"Could not create IHost. The following requirements were not met: {hostConfig.MissingRequirements.ToText()}" );

            var builder = hostConfig.CreateHostBuilder();

            if( builder == null )
                throw new ArgumentException( "Failed to create host builder." );

            var retVal = builder.Build();

            if( retVal == null )
                throw new ArgumentException( "Failed to build host" );

            return retVal;
        }

        private void ConfigurationInitializer(IConfigurationBuilder builder )
        {
            builder.AddUserSecrets<AppConfig>();
        }

        private void LoggerInitializer( IConfiguration config, J4JLoggerConfiguration loggerConfig )
        {
            loggerConfig.SerilogConfiguration.ReadFrom.Configuration( config );
        }

        private void SetupDependencyInjection( HostBuilderContext hbc, ContainerBuilder builder )
        {
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

            builder.Register( c => hbc.Configuration.Get<AppConfig>() )
                .AsSelf()
                .AsImplementedInterfaces()
                .SingleInstance();

            //builder.Register( c =>
            //    {
            //        var appConfig = c.Resolve<IAppConfig>();
            //        var userConfig = c.Resolve<IUserConfig>();
            //        var logger = c.Resolve<IJ4JLogger>();

            //        return new MainVM( appConfig, userConfig, logger );
            //    } )
            //    .AsSelf();

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

        private string FilePathTrimmer(
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

        private string GetProjectPath([CallerFilePath] string filePath = "")
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
