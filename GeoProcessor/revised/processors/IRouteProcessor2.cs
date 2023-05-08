using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace J4JSoftware.GeoProcessor;

public interface IRouteProcessor2 : IMessageBasedTask
{
    string ProcessorName { get; }
    string ApiKey { get; set; }
    TimeSpan RequestTimeout { get; set; }
    List<IImportFilter> ImportFilters { get; }

    Task<ProcessRouteResult> ProcessRoute( List<IImportedRoute> routes, CancellationToken ctx = default );
}
