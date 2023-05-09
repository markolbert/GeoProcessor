using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.GeoProcessor.RouteBuilder;

public class RouteBuilder
{
    private readonly List<DataToImportBase> _dataSources = new();
    private readonly List<IImportFilter> _importFilters = new();
    private readonly List<IExporter2> _exportTargets = new();

    public RouteBuilder(
        ILoggerFactory? loggerFactory = null
    )
    {
        LoggerFactory = loggerFactory;
        Logger = loggerFactory?.CreateLogger<RouteBuilder>();
    }

    public ILoggerFactory? LoggerFactory { get; }
    public ILogger? Logger { get; }

    public IRouteProcessor2? SnapProcessor { get; set; }
    public void AddDataSource( DataToImportBase dataToImport ) => _dataSources.Add( dataToImport );
    public void AddImportFilter( IImportFilter filter ) => _importFilters.Add( filter );
    public void AddExportTarget( IExporter2 exportTarget ) => _exportTargets.Add( exportTarget );

    public void Clear()
    {
        _dataSources.Clear();
        SnapProcessor = null;
    }

    public Color RouteColor { get; internal set; } = Color.Blue;
    public Color RouteHighlightColor { get; internal set; } = Color.Red;
    public int RouteWidth { get; internal set; } = 5;

    public Func<StatusInformation, Task>? StatusReporter { get; internal set; }
    public int StatusInterval { get; internal set; } = GeoConstants.DefaultStatusInterval;
    public Func<ProcessingMessage, Task>? MessageReporter { get; internal set; }

    public async Task<ProcessRouteResult> BuildAsync( CancellationToken ctx = default )
    {
        if( !_dataSources.Any() )
        {
            await SendMessage( "Startup", "Nothing to process" );
            return ProcessRouteResult.Failed;
        }

        if( SnapProcessor == null )
        {
            await SendMessage( "Startup", "No route processor defined" );
            return ProcessRouteResult.Failed;
        }

        var importedRoutes = new List<IImportedRoute>();

        foreach( var curImport in _dataSources )
        {
            curImport.Importer.MessageReporter = MessageReporter;
            curImport.Importer.StatusReporter = StatusReporter;
            curImport.Importer.StatusInterval = StatusInterval;

            importedRoutes.AddRange( await curImport.Importer.ImportAsync( curImport, ctx ) );
        }

        SnapProcessor.ImportFilters.AddRange( _importFilters );

        var retVal = await SnapProcessor.ProcessRoute( importedRoutes, ctx );

        foreach (var exportTarget in _exportTargets)
        {
            await exportTarget.ExportAsync(retVal.Results, ctx);
        }

        return retVal;
    }

    private async Task SendMessage( string phase, string message, bool log = true, LogLevel logLevel = LogLevel.Warning )
    {
        if( MessageReporter != null )
            await MessageReporter( new ProcessingMessage( phase, message ) );

        if( log )
            Logger?.Log( logLevel, message );
    }

    private async Task SendStatus(
        string phase,
        string mesg,
        int totalPoints,
        int processedPts,
        bool log = false,
        LogLevel logLevel = LogLevel.Warning
    )
    {
        if( StatusReporter != null )
            await StatusReporter( new StatusInformation( phase, mesg, totalPoints, processedPts ) );

        if( log )
            Logger?.Log( logLevel, "{phase}{mesg}: {processed} of {total} processed", phase, mesg, processedPts, totalPoints );
    }
}