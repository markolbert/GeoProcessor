using System.Threading;
using System.Threading.Tasks;

namespace J4JSoftware.KMLProcessor
{
    public interface IExport
    {
        ExportType Type { get; }
        string GetNumberedFilePath( int idx );
        Task<bool> ExportAsync( KmlDocument kDoc, int docIndex, CancellationToken cancellationToken );
    }
}
