using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace J4JSoftware.GeoProcessor
{
    public interface IImport
    {
        ImportType Type { get; }

        Task<List<KmlDocument>?> ImportAsync( string filePath, CancellationToken cancellationToken );
    }
}