using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.GeoProcessor;

public abstract class MessageBasedTask : IMessageBasedTask
{
    private int _statusInterval = GeoConstants.DefaultStatusInterval;
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

    public Func<StatusInformation, Task>? StatusReporter { get; set; }

    public Func<ProcessingMessage, Task>? MessageReporter { get; set; }

    public int StatusInterval
    {
        get => _statusInterval;
        set => _statusInterval = value < 0 ? GeoConstants.DefaultStatusInterval : value;
    }

    protected virtual async Task OnProcessingStarted(
        string? mesg = null,
        bool log = false,
        LogLevel level = LogLevel.Information
    )
    {
        if( string.IsNullOrEmpty( mesg ) )
            mesg = "starting";

        _itemsProcessed = 0;
        _itemsProcessedSinceLastUpdate = 0;

        await SendMessage( ExpandedPhase, mesg, log, level );
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
        bool log = true,
        LogLevel logLevel = LogLevel.Warning
    )
    {
        if( MessageReporter != null )
            await MessageReporter( new ProcessingMessage( phase, message ) );

        if( log )
            Logger?.Log( logLevel, message );
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
            await StatusReporter( new StatusInformation( phase, mesg, itemsToProcess, itemsProcessed ) );

        if( log )
            Logger?.Log( logLevel,
                         "{phase}{mesg}: {processed} of {total} processed",
                         phase,
                         mesg,
                         itemsProcessed,
                         itemsToProcess );
    }
}
