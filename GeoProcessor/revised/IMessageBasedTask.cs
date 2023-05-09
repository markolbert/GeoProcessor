using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace J4JSoftware.GeoProcessor;

public interface IMessageBasedTask
{
    Func<StatusInformation, Task>? StatusReporter { get; set; }
    Func<ProcessingMessage, Task>? MessageReporter { get; set; }
    int StatusInterval { get; set; }
    ReadOnlyCollection<string> ProblemMessages { get; }
}
