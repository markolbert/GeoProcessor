using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using J4JSoftware.Logging;
using Microsoft.Extensions.Hosting;

namespace J4JSoftware.KMLProcessor
{
    public class StoreKeyApp : IHostedService
    {
        private readonly AppConfig _config;
        private readonly IHost _host;
        private readonly IHostApplicationLifetime _lifetime;
        private readonly IJ4JLogger _logger;

        public StoreKeyApp(
            IHost host,
            AppConfig config,
            IHostApplicationLifetime lifetime,
            IJ4JLogger logger
        )
        {
            _host = host;
            _config = config;
            _lifetime = lifetime;

            _logger = logger;
            _logger.SetLoggedType( GetType() );
        }

        public async Task StartAsync( CancellationToken cancellationToken )
        {
            if( !_config.IsValid( _logger ) )
            {
                _lifetime.StopApplication();
                return;
            }

            if( !_config.StoreAPIKey )
            {
                _lifetime.StopApplication();
                return;
            }

            var secureProcessors = _config.Processors
                                       ?.Where( p => p.Key.IsSecuredProcessor() )
                                       .Select( p => p.Key )
                                       .ToList()
                                   ?? new List<ProcessorType>();

            if( !secureProcessors.Any() )
            {
                _logger.Error("No processors are defined");
                _lifetime.StopApplication();

                return;
            }

            Console.WriteLine("Select the processor whose API key you want to encrypt and store:\n");

            var idx = 0;

            foreach( var pType in secureProcessors )
            {
                Console.WriteLine($"{idx+1} - {pType}");

                idx++;
            }

            Console.Write("\nChoice: ");
            var procNumText = Console.ReadLine();

            if( !int.TryParse(procNumText, out var procNum) 
                || procNum < 1 
                || procNum > secureProcessors.Count)
            {
                _logger.Error("Invalid choice");
                _lifetime.StopApplication();

                return;
            }

            var procType = secureProcessors.Skip( procNum - 1 ).First();

            Console.Write( $"Enter the {procType} API key: " );
            var apiKey = Console.ReadLine();

            if( string.IsNullOrEmpty( apiKey ) )
            {
                _logger.Error( "Key is undefined, configuration not updated" );
                _lifetime.StopApplication();
            }

            var tempConfig = new AppConfig { APIKeys = _config.APIKeys ?? new Dictionary<ProcessorType, APIKey>() };

            var newKey = new APIKey
            {
                Type = procType,
                Value = apiKey!
            };

            if( !tempConfig.APIKeys.ContainsKey( procType ) )
                tempConfig.APIKeys.Add( procType, newKey );
            else tempConfig.APIKeys[ procType ] = newKey;

            var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
            jsonOptions.Converters.Add( new JsonStringEnumConverter() );
            jsonOptions.Converters.Add(new APIKeysConverter());

            var serialized = JsonSerializer.Serialize( tempConfig, jsonOptions);

            await File.WriteAllTextAsync( 
                Path.Combine( Program.AppUserFolder, Program.UserConfigFile ), 
                serialized,
                cancellationToken );

            _logger.Information( "{0} API key updated", procType );

            _lifetime.StopApplication();
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task StopAsync( CancellationToken cancellationToken )
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
        }
    }
}