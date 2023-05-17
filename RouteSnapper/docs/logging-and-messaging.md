# Logging and Messaging

Provided you enable logging by providing an instance of `ILoggerFactory` when you create a `RouteBuilder` instance, the library raises many logging events.

It also generates various progress and status messages. You handle these by specifying handler methods and the frequency with which you want progress messages sent.

|Extension Method|Arguments|Comments|
|----------------|---------|--------|
|`SendProgressReportsTo`|`Func<ProgressInformation, Task>` progressReporter|specifies the handler for progress reports. If you don't specify one no progress reports will be available. The handler must be an `async` method (even if it does not contain any `await` calls).|
|`SendStatusReportsTo`|`Func<StatusReport, Task>` statusReporter|specifies the handler for status reports. If you don't specify one no status reports will be available. The handler must be an `async` method (even if it does not contain any `await` calls).|
|`ProgressInterval`|`int` interval|specifies the frequency with which progress reports are generated, in terms of processing events (e.g., a value of 100 means a progress report will be generated every 100 actions taken by some process). Defaults to 500.|

The objects used to send information are pretty self-explanatory:

```csharp
public record StatusInformation( 
    string Phase, 
    string Message, 
    int TotalToProcess, 
    int Processed );

public record ProcessingMessage( 
    string Phase, 
    string Message );
```

[return to configuration](overview.md#configuration-via-extension-methods)

[return to overview](overview.md#j4jsoftwaregeoprocessor-overview)
