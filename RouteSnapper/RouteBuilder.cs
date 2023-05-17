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

namespace J4JSoftware.RouteSnapper.RouteBuilder;

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

    public IRouteProcessor? SnapProcessor { get; internal set; }
    internal void AddDataSource( DataToImportBase dataToImport ) => _dataSources.Add( dataToImport );
    internal void AddImportFilter( IImportFilter filter ) => _importFilters.Add( filter );
    internal void AddExportTarget( IExporter exportTarget ) => _exportTargets.Add( exportTarget );

    public Func<ProgressInformation, Task>? ProgressReporter { get; internal set; }
    public int ProgressInterval { get; internal set; } = GeoConstants.DefaultProgressInterval;
    public Func<StatusReport, Task>? StatusReporter { get; internal set; }

    public async Task<BuildResults> BuildAsync( CancellationToken ctx = default )
    {
        var retVal = new BuildResults();

        if( !_dataSources.Any() )
        {
            await SendMessage( "Startup", "Nothing to process" );
            return retVal;
        }

        if( SnapProcessor == null )
        {
            await SendMessage( "Startup", "No route processor defined" );
            return retVal;
        }

        retVal.ImportedRoutes = new List<Route>();

        foreach( var curImport in _dataSources )
        {
            curImport.Importer.MessageReporter = StatusReporter;
            curImport.Importer.StatusReporter = ProgressReporter;
            curImport.Importer.StatusInterval = ProgressInterval;

            var curRoutes = await curImport.Importer.ImportAsync( curImport, ctx );

            if( curRoutes != null )
                retVal.ImportedRoutes.AddRange( curRoutes );
        }

        SnapProcessor.ImportFilters.AddRange( _importFilters );

        var temp = await SnapProcessor.ProcessRoute( retVal.ImportedRoutes, ctx );
        retVal.FilteredRoutes = temp.FilteredRoutes;
        retVal.SnappedRoutes = temp.SnappedRoutes;
        retVal.Problems = temp.Problems;

        if( !retVal.Succeeded )
            return retVal;

        foreach (var exportTarget in _exportTargets)
        {
            await exportTarget.ExportAsync(retVal.SnappedRoutes!, ctx);
        }

        return retVal;
    }

    private async Task SendMessage( string phase, string message, bool log = true, LogLevel logLevel = LogLevel.Warning )
    {
        if( StatusReporter != null )
            await StatusReporter( new StatusReport( phase, message ) );

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
        if( ProgressReporter != null )
            await ProgressReporter( new ProgressInformation( phase, mesg, totalPoints, processedPts ) );

        if( log )
            Logger?.Log( logLevel, "{phase}{mesg}: {processed} of {total} processed", phase, mesg, processedPts, totalPoints );
    }
}