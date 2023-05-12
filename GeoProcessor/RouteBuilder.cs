#region copyright
// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// RouteBuilder.cs
//
// This file is part of JumpForJoy Software's GeoProcessor.
// 
// GeoProcessor is free software: you can redistribute it and/or modify it 
// under the terms of the GNU General Public License as published by the 
// Free Software Foundation, either version 3 of the License, or 
// (at your option) any later version.
// 
// GeoProcessor is distributed in the hope that it will be useful, but 
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
// or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License 
// for more details.
// 
// You should have received a copy of the GNU General Public License along 
// with GeoProcessor. If not, see <https://www.gnu.org/licenses/>.
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.GeoProcessor.RouteBuilder;

public class RouteBuilder
{
    private readonly List<DataToImportBase> _dataSources = new();
    private readonly List<IImportFilter> _importFilters = new();
    private readonly List<IExporter> _exportTargets = new();

    public RouteBuilder(
        ILoggerFactory? loggerFactory = null
    )
    {
        LoggerFactory = loggerFactory;
        Logger = loggerFactory?.CreateLogger<RouteBuilder>();

        // add default filters
        _importFilters.Add( new RemoveSinglePointRoutes( loggerFactory ) );
    }

    public ILoggerFactory? LoggerFactory { get; }
    public ILogger? Logger { get; }

    public IRouteProcessor? SnapProcessor { get; set; }
    public void AddDataSource( DataToImportBase dataToImport ) => _dataSources.Add( dataToImport );
    public void AddImportFilter( IImportFilter filter ) => _importFilters.Add( filter );
    public void AddExportTarget( IExporter exportTarget ) => _exportTargets.Add( exportTarget );

    public void Clear()
    {
        _dataSources.Clear();
        SnapProcessor = null;
    }

    public Func<StatusInformation, Task>? StatusReporter { get; internal set; }
    public int StatusInterval { get; internal set; } = GeoConstants.DefaultStatusInterval;
    public Func<ProcessingMessage, Task>? MessageReporter { get; internal set; }

    public async Task<List<ImportedRoute>?> BuildAsync( CancellationToken ctx = default )
    {
        if( !_dataSources.Any() )
        {
            await SendMessage( "Startup", "Nothing to process" );
            return null;
        }

        if( SnapProcessor == null )
        {
            await SendMessage( "Startup", "No route processor defined" );
            return null;
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
            exportTarget.AddFilters( _importFilters.Where( x => x.Category == ImportFilterCategory.PostSnapping ) );

            await exportTarget.ExportAsync(retVal, ctx);
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