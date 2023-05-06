using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace J4JSoftware.GeoProcessor;

public interface IRouteProcessor2 : IMessageBasedTask
{
    string ProcessorName { get; }
    string ApiKey { get; set; }
    double MaxPointSeparation { get; set; }
    TimeSpan RequestTimeout { get; set; }

    Task<ProcessRouteResult> ProcessRoute( List<ImportedRoute> routes, CancellationToken ctx = default );
}
