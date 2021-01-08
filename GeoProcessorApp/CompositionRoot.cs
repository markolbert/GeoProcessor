using System;
using System.IO;
using System.Text;
using Autofac;
using J4JSoftware.Configuration.CommandLine;
using J4JSoftware.DependencyInjection;
using J4JSoftware.Logging;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace J4JSoftware.GeoProcessor
{
    public class CompositionRoot : J4JCompositionRoot<J4JLoggerConfiguration>
    {
        public static CompositionRoot Default { get; }

        static CompositionRoot()
        {
            Default = new CompositionRoot()
            {
                CachedLoggerScope = CachedLoggerScope.SingleInstance,
                LoggingSectionKey = "Logging",
                UseConsoleLifetime = true
            };

            Default.ChannelInformation
                .AddChannel<ConsoleConfig>( "Logging:Channels:Console" )
                .AddChannel<DebugConfig>( "Logging:Channels:Debug" );

            Default.Initialize();
        }

        private CompositionRoot()
        {
        }

        public bool Protect( string plainText, out string? encrypted )
        {
            encrypted = null;

            var dataProtection = Host?.Services.GetService<IDataProtection>();
            if( dataProtection == null )
                return false;

            var utf8 = new UTF8Encoding();
            var bytesToEncrypt = utf8.GetBytes(plainText);

            try
            {
                var encryptedBytes = dataProtection.Protector.Protect( bytesToEncrypt );
                encrypted = Convert.ToBase64String( encryptedBytes );
            }
#pragma warning disable 168
            catch( Exception e )
#pragma warning restore 168
            {
                return false;
            }

            return true;
        }

        public bool Unprotect( string encryptedText, out string? decrypted )
        {
            decrypted = null;

            var dataProtection = Host?.Services.GetService<IDataProtection>();
            if (dataProtection == null)
                return false;

            byte[] decryptedBytes;

            try
            {
                var encryptedBytes = Convert.FromBase64String( encryptedText );
                decryptedBytes = dataProtection.Protector.Unprotect( encryptedBytes );
            }
#pragma warning disable 168
            catch( Exception e )
#pragma warning restore 168
            {
                return false;
            }

            var utf8 = new UTF8Encoding();
            decrypted = utf8.GetString( decryptedBytes );

            return true;
        }

        protected override void SetupConfigurationEnvironment( IConfigurationBuilder builder )
        {
            base.SetupConfigurationEnvironment( builder );

            var junk = GetCachedLogger();

            var options = new OptionCollection(CommandLineStyle.Linux, () => GetCachedLogger()!);

            options.Bind<AppConfig, string>(x => x.InputFile.FilePath, "i", "inputFile");
            options.Bind<AppConfig, string>(x => x.DefaultRouteName, "n", "defaultName");
            options.Bind<AppConfig, string>(x => x.OutputFile.FilePath, "o", "outputFile");
            options.Bind<AppConfig, ExportType>(x => x.ExportType, "t", "outputType");
            options.Bind<AppConfig, bool>(x => x.StoreAPIKey, "k", "storeApiKey");
            options.Bind<AppConfig, bool>(x => x.RunInteractive, "r", "runInteractive");
            options.Bind<AppConfig, ProcessorType>(x => x.ProcessorType, "p", "snapProcessor");

            builder.SetBasePath(Environment.CurrentDirectory)
                .AddJsonFile(Path.Combine(Environment.CurrentDirectory, Program.AppConfigFile), false, false)
                .AddJsonFile(Path.Combine(Program.AppUserFolder, Program.UserConfigFile), true, false)
                .AddUserSecrets<AppConfig>()
                .AddJ4JCommandLine(options);
        }

        protected override void SetupDependencyInjection( HostBuilderContext hbc, ContainerBuilder builder )
        {
            base.SetupDependencyInjection( hbc, builder );

            builder.Register(c =>
            {
                AppConfig? config = null;

                try
                {
                    config = hbc.Configuration.Get<AppConfig>();
                }
                catch (Exception e)
                {
                    GetCachedLogger()!.Fatal<string>(
                        "Failed to parse configuration information. Message was: {0}",
                        e.Message);
                }

                config ??= new AppConfig();

                // validate, but not when all we're doing is storing an API key
                if (!config.StoreAPIKey)
                {
                    var validator = c.Resolve<IAppConfigValidator>();
                    validator.Validate( config );
                }

                if (!string.IsNullOrEmpty(config.OutputFile.FileNameWithoutExtension))
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
            })
                .AsImplementedInterfaces()
                .AsSelf()
                .SingleInstance();

            builder.RegisterType<AppConfigValidator>()
                .As<IAppConfigValidator>()
                .SingleInstance();

            builder.RegisterModule<AutofacGeoProcessorModule>();
        }

        protected override void SetupServices( HostBuilderContext hbc, IServiceCollection services )
        {
            base.SetupServices( hbc, services );

            services.AddDataProtection();

            var config = hbc.Configuration.Get<AppConfig>();

            if (config!.StoreAPIKey)
                services.AddHostedService<StoreKeyApp>();
            else
                services.AddHostedService<RouteApp>();
        }
    }
}
