using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Alba.CsConsoleFormat.Fluent;
using Autofac.Features.Indexed;
using J4JSoftware.ConsoleUtilities;
using J4JSoftware.Logging;
using Microsoft.Extensions.Hosting;

namespace J4JSoftware.GeoProcessor
{
    public class StoreKeyApp : IHostedService
    {
        internal const string AutofacKey = "StoreKeyApp";

        private readonly AppConfig _config;
        private readonly IHostApplicationLifetime _lifetime;
        private readonly IJ4JLogger _logger;

        public StoreKeyApp(
            AppConfig config,
            IHostApplicationLifetime lifetime,
            IIndex<string, IConfigurationUpdater<AppConfig>> configUpdaters,
            IJ4JLogger logger
        )
        {
            _config = config;
            _lifetime = lifetime;

            _logger = logger;
            _logger.SetLoggedType( GetType() );

            if( !configUpdaters.TryGetValue( AutofacKey, out var updater ) || !updater.Update( _config ) )
                return;

            _logger.Fatal( "Incomplete configuration, aborting" );
            _lifetime.StopApplication();
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


            Colors.WriteLine( "Select the ", "processor ".Yellow(), " whose API key you want to encrypt and store:\n" );

            var procType =
                ConsoleExtensions.GetEnum<ProcessorType>( _config.ProcessorType,
                    _config.ProcessorType switch
                    {
                        ProcessorType.Distance => ProcessorType.Google,
                        ProcessorType.Undefined => ProcessorType.Google,
                        _ => _config.ProcessorType
                    },
                    secureProcessors );

            var apiKey =
                ConsoleExtensions.GetText(_config.APIKey,
                    _config.APIKey, 
                    "the ".White(), 
                    $"{procType}".Green(),
                    " API key".White() );

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