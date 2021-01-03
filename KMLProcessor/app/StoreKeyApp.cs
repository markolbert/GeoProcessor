﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using J4JSoftware.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

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
            IOptions<AppConfig> config,
            IHostApplicationLifetime lifetime,
            IJ4JLogger logger
        )
        {
            _host = host;
            _config = config.Value;
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

            Console.Write( $"Enter the {_config.SnapProcessorType} API key: " );
            var apiKey = Console.ReadLine();

            if( string.IsNullOrEmpty( apiKey ) )
            {
                _logger.Error( "Key is undefined, configuration not updated" );
                _lifetime.StopApplication();
            }

            if( !KMLExtensions.Encrypt( apiKey!, out var encrypted ) )
            {
                _logger.Error( "Couldn't encrypt API key, configuration not updated" );
                _lifetime.StopApplication();
            }

            var apiConfig = _config.APIKeys.FirstOrDefault( k => k.Type == _config.SnapProcessorType );

            if( apiConfig == null )
                _config.APIKeys.Add(
                    new SnapProcessorAPIKey
                    {
                        Type = _config.SnapProcessorType,
                        EncryptedAPIKey = encrypted!
                    } );
            else apiConfig.EncryptedAPIKey = encrypted!;

            var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
            jsonOptions.Converters.Add( new JsonStringEnumConverter() );

            var serialized = JsonSerializer.Serialize( _config, jsonOptions);

            await File.WriteAllTextAsync( Program.AppUserConfigFile, serialized, cancellationToken );

            _logger.Information( "{0} API key updated", _config.SnapProcessorType );

            _lifetime.StopApplication();
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task StopAsync( CancellationToken cancellationToken )
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
        }
    }
}