using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Autofac;
using J4JSoftware.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace J4JSoftware.KMLProcessor
{
    class Program
    {
        private static Dictionary<string, string> _cmdLineMappings =
            new Dictionary<string, string>()
            {
                { "-i", "Configuration:InputFile" },
                { "--inputFile", "Configuration:InputFile" },
                { "-o", "Configuration:OutputFile" },
                { "--outputFile", "Configuration:OutputFile" },
                { "-m", "Configuration:CoalesceValue" },
                { "--minDistanceValue", "Configuration:CoalesceValue" },
                { "-u", "Configuration:CoalesceUnit" },
                { "--minDistanceUnit", "Configuration:CoalesceUnit" }
            };

        static async Task Main(string[] args)
        {
            var hostBuilder = InitializeHostBuilder();

            await hostBuilder.RunConsoleAsync();
        }

        private static IHostBuilder InitializeHostBuilder()
        {
            var retVal = new J4JHostBuilder();

            retVal.AddJ4JLogging<LoggingChannelConfig>();

            retVal.ConfigureHostConfiguration(builder =>
            {
                builder
                    .SetBasePath(Environment.CurrentDirectory)
                    .AddUserSecrets<AppConfig>()
                    .AddJsonFile(Path.Combine(Environment.CurrentDirectory, "appConfig.json"), false, false)
                    .AddCommandLine(Environment.GetCommandLineArgs(), _cmdLineMappings);
            });

            retVal.ConfigureContainer<ContainerBuilder>((context, builder) =>
            {
                builder.Register(c =>
                    {
                        var temp = context.Configuration
                            .GetSection("Configuration")
                            .Get<AppConfig>();

                        return temp ?? new AppConfig();
                    })
                    .AsImplementedInterfaces()
                    .SingleInstance();

                builder.RegisterType<KmlDocument>()
                    .AsSelf();

                builder.RegisterType<BingSnapRouteProcessor>()
                    .AsImplementedInterfaces();
            });

            retVal.ConfigureServices((context, services) =>
            {
                services.AddHostedService<App>();
            });

            return retVal;
        }
    }
}
