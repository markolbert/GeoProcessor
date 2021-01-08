using System.Threading;
using System.Threading.Tasks;

namespace J4JSoftware.GeoProcessor
{
    public interface IExport
    {
        ExportType Type { get; }
        Task<bool> ExportAsync( KmlDocument kDoc, int docIndex, CancellationToken cancellationToken );
    }
}
