using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace J4JSoftware.GeoProcessor
{
    public interface IExporter
    {
        ExportType Type { get; }
        Task<bool> ExportAsync( PointSet pointSet, int docIndex, CancellationToken cancellationToken );
    }
}
