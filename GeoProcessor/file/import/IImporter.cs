using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace J4JSoftware.GeoProcessor
{
    public interface IImporter
    {
        ImportType Type { get; }
        Task<List<PointSet>?> ImportAsync( string filePath, CancellationToken cancellationToken );
    }
}