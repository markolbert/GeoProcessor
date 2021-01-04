using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace J4JSoftware.KMLProcessor
{
    public interface IImportExport
    {
        FileType Type { get; }

        Task<KmlDocument?> ImportAsync( string filePath, CancellationToken cancellationToken );

        Task<bool> ExportAsync(
            KmlDocument kDoc,
            string filePath,
            CancellationToken cancellationToken );
    }
}
