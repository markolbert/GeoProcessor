using System.Threading;
using System.Threading.Tasks;
using Autofac.Features.Indexed;
using J4JSoftware.Configuration.CommandLine;
using J4JSoftware.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace J4JSoftware.KMLProcessor
{
    public class RouteApp : IHostedService
    {
        private readonly AppConfig _config;
        private readonly IHost _host;
        private readonly IHostApplicationLifetime _lifetime;
        private readonly IRouteProcessor _distProc;
        private readonly IRouteProcessor _routeProc;
        private readonly IJ4JLogger _logger;

        public RouteApp(
            IHost host,
            AppConfig config,
            IHostApplicationLifetime lifetime,
            IIndex<ProcessorType, IRouteProcessor> snapProcessors,
            IJ4JLogger logger
        )
        {
            _host = host;
            _config = config;
            _lifetime = lifetime;

            _distProc = snapProcessors[ProcessorType.Distance];
            _routeProc = snapProcessors[config.ProcessorType];

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

            if( _config.StoreAPIKey )
                return;

            var kDoc = _host.Services.GetRequiredService<KmlDocument>();

            if( !await kDoc.LoadAsync( _config.InputFile!, cancellationToken ) )
                return;

            var prevPts = kDoc.Points.Count;

            if( !await RunRouteProcessor( kDoc, _distProc, cancellationToken ) )
            {
                _lifetime.StopApplication();
                return;
            }

            _logger.Information( "Reduced points from {0:n0} to {1:n0} by coalescing nearby points", 
                prevPts,
                kDoc.Points.Count );

            prevPts = kDoc.Points.Count;

            if( !await RunRouteProcessor( kDoc, _routeProc, cancellationToken ) )
            {
                _lifetime.StopApplication();
                return;
            }

            _logger.Information( "Snapping to route changed point count from {0:n0} to {1:n0}", 
                prevPts,
                kDoc.Points.Count );

            if( !await kDoc.SaveAsync( _config.OutputFile!, cancellationToken ) )
                return;

            _logger.Information<string>( "Wrote file '{0}'", _config.OutputFile! );

            _lifetime.StopApplication();
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task StopAsync( CancellationToken cancellationToken )
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            _lifetime.StopApplication();
        }

        private async Task<bool> RunRouteProcessor( KmlDocument kDoc, IRouteProcessor processor, CancellationToken cancellationToken)
        {
            var routePts = await processor.ProcessAsync(kDoc.Points, cancellationToken);

            if (routePts == null)
            {
                _logger.Error("Route processor '{0}' failed", processor.GetType());
                return false;
            }

            kDoc.Points = routePts;

            return true;
        }

    }
}