using System;
using System.IO;
using System.Linq;
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
        private readonly IIndex<FileType, IImportExport> _fileProcessors;
        private readonly IRouteProcessor _distProc;
        private readonly IRouteProcessor _routeProc;
        private readonly IJ4JLogger _logger;

        public RouteApp(
            IHost host,
            AppConfig config,
            IHostApplicationLifetime lifetime,
            IIndex<FileType, IImportExport> fileProcessors,
            IIndex<ProcessorType, IRouteProcessor> snapProcessors,
            IJ4JLogger logger
        )
        {
            _host = host;
            _config = config;
            _lifetime = lifetime;
            _fileProcessors = fileProcessors;

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

            // import the file; start by seeing if we can determine the file type
            // from the file extension
            FileType? fileType = null;
            var fileExt = Path.GetExtension( _config.InputFile );

            if( fileExt != null 
                && Enum.TryParse( typeof(FileType), fileExt[1..], true, out var temp ) )
                fileType = (FileType) temp!;

            fileType ??= FileType.Unknown;

            KmlDocument? kDoc = null;

            if( fileType != FileType.Unknown )
                kDoc = await LoadFileAsync( fileType.Value, cancellationToken );

            // if the import based on the extension failed, try all our importers
            if( kDoc == null )
            {
                foreach( var fType in Enum.GetValues<FileType>()
                    .Where( ft => ft != fileType && ft != FileType.Unknown ) )
                {
                    kDoc = await LoadFileAsync( fType, cancellationToken );

                    if( kDoc == null ) 
                        continue;

                    fileType = fType;
                    break;
                }
            }

            if( kDoc == null )
            {
                _logger.Error<string>( "Could not load file '{0}'", _config.InputFile! );

                _lifetime.StopApplication();
                return;
            }

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

            var exporter = _fileProcessors[ fileType.Value ]!;

            if( await exporter.ExportAsync( kDoc, _config.OutputFile!, cancellationToken ) )
                _logger.Information<string>( "Wrote file '{0}'", _config.OutputFile! );
            else _logger.Information<string>("Export to file '{0}' failed", _config.OutputFile!);

            _lifetime.StopApplication();
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task StopAsync( CancellationToken cancellationToken )
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            _lifetime.StopApplication();
        }

        private async Task<KmlDocument?> LoadFileAsync( FileType fileType, CancellationToken cancellationToken )
        {
            if( !_fileProcessors.TryGetValue( fileType, out var importer ) )
                return null;

            return await importer!.ImportAsync( _config.InputFile!, cancellationToken );
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