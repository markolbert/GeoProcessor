using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using J4JSoftware.Logging;

namespace J4JSoftware.GeoProcessor
{
    public class MockAppConfig : IAppConfig
    {
        public MockAppConfig()
        {
            OutputFile = new()
            {
                FilePath = "c:/test output file.kmz"
            };

            InputFile = new()
            {
                FilePath = "c:/test input file.kml"
            };
        }

        public ProcessorType ProcessorType { get; set; } = ProcessorType.Google;
        public ProcessorInfo? ProcessorInfo { get; }
        public string APIKey { get; set; } = "an API key";
        public OutputFileInfo OutputFile { get; }
        public int RouteWidth { get; set; } = 5;
        public Color RouteColor { get; set; } = Color.Chocolate;
        public Color RouteHighlightColor { get; set; } =Color.Aqua;
        public string ApplicationConfigurationFolder { get; set; } = string.Empty;
        public string UserConfigurationFolder { get; set; } = string.Empty;
        public NetEventSink? NetEventSink { get; set; }
        public InputFileInfo InputFile { get; }
        public Dictionary<ProcessorType, ProcessorInfo> Processors { get; set; } = new();
        public ExportType ExportType { get; set; } = ExportType.KML;

        public void RestoreFrom( CachedAppConfig src )
        {
            throw new NotImplementedException();
        }
    }
}
