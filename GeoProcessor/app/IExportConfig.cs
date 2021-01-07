using System.Drawing;

namespace J4JSoftware.GeoProcessor
{
    public interface IExportConfig : IGeoConfig
    {
        OutputFileInfo OutputFile { get; }
        int RouteWidth { get; } 
        Color RouteColor { get; }
        Color RouteHighlightColor { get; }
    }
}