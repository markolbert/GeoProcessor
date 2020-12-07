using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using J4JSoftware.Logging;
using Microsoft.Extensions.Hosting;

namespace J4JSoftware.KMLProcessor
{
    public class App : IHostedService
    {
        private readonly IHost _host;
        private readonly IAppConfig _config;
        private readonly IHostApplicationLifetime _lifetime;
        private readonly IJ4JLogger _logger;

        public App( 
            IHost host,
            IAppConfig config,
            IHostApplicationLifetime lifetime,
            IJ4JLogger logger
        )
        {
            _host = host;
            _config = config;
            _lifetime = lifetime;

            _logger = logger;
            _logger.SetLoggedType( this.GetType() );
        }

        public async Task StartAsync( CancellationToken cancellationToken )
        {
            if( !_config.IsValid( out var error ) )
            {
                abort( error! );
                return;
            }

            using var stream = File.OpenText( _config.InputFile );
            var xDoc = await XDocument.LoadAsync( stream, LoadOptions.None, cancellationToken );

            var rawCoords = xDoc.Descendants()
                .SingleOrDefault( x => x.Name.LocalName == "coordinates" );

            var coordText = rawCoords!.Value.Replace( "\t", "" )
                .Replace( "\n", "" );

            var coordinates = coordText.Split( ' ', StringSplitOptions.RemoveEmptyEntries )
                .Select( t => new Coordinate( t ) )
                .ToList();

            var minMoveIndex = -1;
            Coordinate? prior = null;
            var deDuped = new List<Coordinate>();

            foreach( var coord in coordinates )
            {
                if( prior == null )
                {
                    prior = coord;
                    deDuped.Add( coord );

                    continue;
                }

                if( coord.MinDelta( prior ) == 0.0 )
                    continue;

                deDuped.Add( coord );
                prior = coord;
            }

            var minDelta = double.MaxValue;

            for( var idx = 0; idx < deDuped.Count; idx++ )
            {
                var coord = deDuped[ idx ];

                if( prior == null )
                {
                    prior = coord;
                    continue;
                }

                var delta = coord.MinDelta( prior );

                if( delta < minDelta )
                {
                    minDelta = delta;
                    minMoveIndex = idx;
                }

                prior = coord;
            }

            _logger.Information( "Done!" );

            _lifetime.StopApplication();

            void abort( string mesg )
            {
                _logger.Fatal(mesg);
                _lifetime.StopApplication();
            }
        }

        public async Task StopAsync( CancellationToken cancellationToken )
        {
        }
    }
}