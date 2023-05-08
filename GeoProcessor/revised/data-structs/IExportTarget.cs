using System.Threading.Tasks;

namespace J4JSoftware.GeoProcessor.RouteBuilder;

public interface IExportTarget
{
    void SetRouteData( ExportedRoute route );
    Task SetRouteDataAsync( ExportedRoute route );
}
