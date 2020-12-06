using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Autofac;
using J4JSoftware.DependencyInjection;
using J4JSoftware.Excel;
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
                { "-k", "Configuration:KmlFile" },
                { "--KmlFile", "Configuration:KmlFile" },
                { "-o", "Configuration:OutputFolder" },
                { "--outputPath", "Configuration:OutputFolder" }
            };

        static async Task Main(string[] args)
        {
            var hostBuilder = new J4JHostBuilder();

            hostBuilder.AddJ4JLogging<LoggingChannelConfig>();

            hostBuilder.ConfigureHostConfiguration(builder =>
            {
                builder
                    .SetBasePath(Environment.CurrentDirectory)
                    .AddJsonFile(Path.Combine(Environment.CurrentDirectory, "appConfig.json"), false, false)
                    .AddCommandLine(args, _cmdLineMappings);
            });

            hostBuilder.ConfigureContainer<ContainerBuilder>((context, builder) =>
            {
                builder.Register(c =>
                    {
                        var retVal = context.Configuration
                            .GetSection("Configuration")
                            .Get<AppConfig>();

                        return retVal ?? new AppConfig();
                    })
                    .AsImplementedInterfaces()
                    .SingleInstance();

                builder.RegisterModule(new ExcelExportAutofacModule());
            });

            hostBuilder.ConfigureServices((context, services) =>
            {
                services.AddHostedService<App>();
            });

            await hostBuilder.RunConsoleAsync();
        }
    }
}
