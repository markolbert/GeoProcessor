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

            using var stream = File.OpenText( _config.KmlFile );
            var xDoc = await XDocument.LoadAsync( stream, LoadOptions.None, cancellationToken );
            var junk = xDoc.Descendants().SingleOrDefault( x => x.Name.LocalName == "coordinates" );

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