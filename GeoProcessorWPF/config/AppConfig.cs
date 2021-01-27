using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using J4JSoftware.Logging;

namespace J4JSoftware.GeoProcessor
{
    public class AppConfig : IAppConfig
    {
        public AppConfig()
        {
            OutputFile.FilePath = Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.Desktop ),
                "GeoProcessor.kml" );
        }

        public InputFileInfo InputFile { get; } = new();
        public OutputFileInfo OutputFile { get; } = new();

        public ProcessorType ProcessorType { get; set; } = ProcessorType.Undefined;
        public Dictionary<ProcessorType, ProcessorInfo> Processors { get; set; } =
            new();
        public ProcessorInfo ProcessorInfo => Processors.ContainsKey( ProcessorType )
            ? Processors[ ProcessorType ]
            : new ProcessorInfo();

        public string APIKey { get; set; } = string.Empty;

        public ExportType ExportType
        {
            get => OutputFile.Type;
            set => OutputFile.Type = value;
        }

        public int RouteWidth { get; set; } = 4;
        public Color RouteColor { get; set; } = Color.Red;
        public Color RouteHighlightColor { get; set; } = Color.DarkTurquoise;

        public J4JLoggerConfiguration? Logging { get; set; }
        }
}