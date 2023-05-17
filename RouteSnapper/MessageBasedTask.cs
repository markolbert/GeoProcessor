#region copyright
// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// MessageBasedTask.cs
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
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.RouteSnapper;

public abstract class MessageBasedTask : IMessageBasedTask
{
    private readonly List<string> _problems = new();

    private int _statusInterval = GeoConstants.DefaultProgressInterval;
    private int _itemsProcessedSinceLastUpdate;
    private int _itemsProcessed;

    protected MessageBasedTask(
        string? mesgPrefix = null,
        ILoggerFactory? loggerFactory = null
    )
    {
        mesgPrefix = string.IsNullOrEmpty(mesgPrefix) ? GetType().Name : mesgPrefix;
        MessagePrefix = mesgPrefix;

        var type = GetType();
        Logger = loggerFactory?.CreateLogger(type);
    }

    protected ILogger? Logger { get; }
    protected string MessagePrefix { get; }

    protected string Phase { get; set; } = string.Empty;

    protected string ExpandedPhase =>
        string.IsNullOrEmpty( Phase ) ? $"{MessagePrefix}:" : $"{MessagePrefix}:{Phase}:";

    public Func<ProgressInformation, Task>? StatusReporter { get; set; }

    public Func<StatusReport, Task>? MessageReporter { get; set; }

    public int StatusInterval
    {
        get => _statusInterval;
        set => _statusInterval = value < 0 ? GeoConstants.DefaultProgressInterval : value;
    }

    public ReadOnlyCollection<string> ProblemMessages => _problems.AsReadOnly();

    protected virtual async Task OnProcessingStarted(
        string? mesg = null,
        int itemsToProcess = 0,
        bool log = false,
        LogLevel level = LogLevel.Information
    )
    {
        if( string.IsNullOrEmpty( mesg ) )
            mesg = "starting";

        _itemsProcessed = 0;
        _itemsProcessedSinceLastUpdate = 0;
        _problems.Clear();

        await SendStatus( ExpandedPhase, mesg, itemsToProcess, 0, log, level );
    }

    protected virtual async Task OnItemProcessed( string? mesg = null )
    {
        _itemsProcessed++;
        _itemsProcessedSinceLastUpdate++;

        if( _itemsProcessedSinceLastUpdate < StatusInterval )
            return;

        await SendStatus(ExpandedPhase, mesg ?? string.Empty, -1, _itemsProcessed);
        _itemsProcessedSinceLastUpdate = 0;
    }

    protected virtual async Task OnProcessingEnded( string? mesg = null )
    {
        if( string.IsNullOrEmpty( mesg ) )
            mesg = "process completed";

        await SendStatus( ExpandedPhase, mesg, -1, _itemsProcessed );

        _itemsProcessed = 0;
        _itemsProcessedSinceLastUpdate = 0;
    }

    protected async Task SendMessage(
        string phase,
        string message,
        bool isProblem = false,
        bool log = true,
        LogLevel logLevel = LogLevel.Warning
    )
    {
        if( MessageReporter != null )
            await MessageReporter( new StatusReport( phase, message ) );

        if( log )
            Logger?.Log( logLevel, message );

        if( isProblem )
            _problems.Add( $"{ExpandedPhase} {message}" );
    }

    private async Task SendStatus(
        string phase,
        string mesg,
        int itemsToProcess,
        int itemsProcessed,
        bool log = false,
        LogLevel logLevel = LogLevel.Warning
    )
    {
        if( StatusReporter != null )
            await StatusReporter( new ProgressInformation( phase, mesg, itemsToProcess, itemsProcessed ) );

        if( log )
            Logger?.Log( logLevel,
                         "{phase}{mesg}: {processed} of {total} processed",
                         phase,
                         mesg,
                         itemsProcessed,
                         itemsToProcess );
    }
}
