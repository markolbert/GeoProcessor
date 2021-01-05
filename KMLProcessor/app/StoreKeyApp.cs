using System;
using System.Collections.Generic;
using System.IO;
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
                return;

            Console.Write( $"Enter the {_config.ProcessorType} API key: " );
            var apiKey = Console.ReadLine();

            if( string.IsNullOrEmpty( apiKey ) )
            {
                _logger.Error( "Key is undefined, configuration not updated" );
                _lifetime.StopApplication();
            }

            _config.APIKeys ??= new Dictionary<ProcessorType, APIKey>();

            var newKey = new APIKey
            {
                Type = _config.ProcessorType,
                Value = apiKey!
            };

            if( _config.APIKeys.ContainsKey( _config.ProcessorType ) )
                _config.APIKeys.Add( _config.ProcessorType, newKey );
            else _config.APIKeys[ _config.ProcessorType ] = newKey;

            var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
            jsonOptions.Converters.Add( new JsonStringEnumConverter() );

            var serialized = JsonSerializer.Serialize( _config, jsonOptions);

            await File.WriteAllTextAsync( Program.AppUserConfigFile, serialized, cancellationToken );

            _logger.Information( "{0} API key updated", _config.ProcessorType );

            _lifetime.StopApplication();
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task StopAsync( CancellationToken cancellationToken )
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
        }
    }
}