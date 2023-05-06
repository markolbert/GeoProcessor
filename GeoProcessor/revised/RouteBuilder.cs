﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.GeoProcessor.RouteBuilder;

public class RouteBuilder
{
    private readonly List<DataToImportBase> _toImport = new();
    private readonly FileImporterFactory _importerFactory;
    private readonly DataImporter _dataImporter;
    private readonly RouteProcessorFactory _processorFactory;
    private readonly ILogger? _logger;

    private IRouteProcessor2? _processor;
    private int _numPtCollections = 1;

    public RouteBuilder(
        FileImporterFactory importerFactory,
        RouteProcessorFactory processorFactory,
        ILoggerFactory? loggerFactory = null
    )
    {
        _importerFactory = importerFactory;
        _dataImporter = new DataImporter( loggerFactory );
        _processorFactory = processorFactory;
        _logger = loggerFactory?.CreateLogger<RouteBuilder>();
    }

    public void Clear()
    {
        _toImport.Clear();
        _numPtCollections = 1;
        _processor = null;
    }

    internal bool AddSourceFile(
        string fileType,
        string filePath,
        bool lineStringsOnly,
        double minPointGap,
        double minOverallGap
    )
    {
        var importer = _importerFactory[ fileType ];
        if( importer == null )
            return false;

        if( !File.Exists( filePath ) )
        {
            _logger?.LogError( "{filePath} does not exist", filePath );
            return false;
        }

        importer.LineStringsOnly = lineStringsOnly;

        _toImport.Add( new FileToImport( filePath, importer, minPointGap, minOverallGap ) );
        return true;
    }

    internal bool UseProcessor( string processor, string apiKey, double maxPtSep, TimeSpan requestTimeout )
    {
        var routeProcessor = _processorFactory[ processor ];
        if( routeProcessor == null ) 
            return false;

        _processor = routeProcessor;
        _processor.ApiKey = apiKey;
        _processor.MaxPointSeparation = maxPtSep;
        _processor.RequestTimeout = requestTimeout;

        return true;
    }

    internal void AddCoordinates(
        string name,
        IEnumerable<Coordinate2> coordinates,
        double minPointGap,
        double minOverallGap
    )
    {
        if( string.IsNullOrEmpty( name ) )
        {
            name = $"Collection {_numPtCollections}";
            _numPtCollections++;
        }

        _toImport.Add( new DataToImport( name, coordinates, _dataImporter, minPointGap, minOverallGap ) );
    }

    public Color RouteColor { get; internal set; } = Color.Blue;
    public Color RouteHighlightColor { get; internal set; } = Color.Red;
    public int RouteWidth { get; internal set; } = 5;

    public bool LineStringsOnly { get; internal set; } = true;

    public Func<StatusInformation, Task>? StatusReporter { get; internal set; }
    public int StatusInterval { get; internal set; } = GeoConstants.DefaultStatusInterval;
    public Func<ProcessingMessage, Task>? MessageReporter { get; internal set; }

    public async Task<ProcessRouteResult> BuildAsync( string routeName, CancellationToken ctx = default )
    {
        if( !_toImport.Any() )
        {
            await SendMessage("Startup", "Nothing to process" );
            return ProcessRouteResult.Failed;
        }

        if (_processor == null)
        {
            await SendMessage("Startup", "No route processor defined");
            return ProcessRouteResult.Failed;
        }

        var importedRoutes = new List<ImportedRoute>();

        foreach( var curImport in _toImport )
        {
            curImport.Importer.MessageReporter = MessageReporter;
            curImport.Importer.StatusReporter = StatusReporter;
            curImport.Importer.StatusInterval = StatusInterval;
            curImport.Importer.MaxPointSeparation = _processor.MaxPointSeparation;

            importedRoutes.AddRange( await curImport.Importer.ImportAsync( curImport, ctx ) );
        }

        return await _processor.ProcessRoute( importedRoutes, ctx );
    }

    private async Task SendMessage( string phase, string message, bool log = true, LogLevel logLevel = LogLevel.Warning )
    {
        if( MessageReporter != null )
            await MessageReporter( new ProcessingMessage( phase, message ) );

        if( log )
            _logger?.Log( logLevel, message );
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
            _logger?.Log( logLevel, "{phase}{mesg}: {processed} of {total} processed", phase, mesg, processedPts, totalPoints );
    }
}