
using System.Drawing;

namespace J4JSoftware.GeoProcessor
{
    public interface IExportConfig : IGeoConfig
    {
        public OutputFileInfo OutputFile { get; }
        public int RouteWidth { get; set; }
        public Color RouteColor { get; set; }
        public Color RouteHighlightColor { get; set; }
    }
}