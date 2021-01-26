using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using J4JSoftware.Logging;

namespace J4JSoftware.GeoProcessor
{
    public class AppConfigDesignTime : IAppConfig
    {
        public AppConfigDesignTime()
        {
            OutputFile.FilePath = Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.Desktop ),
                "GeoProcessor.kml" );

            Processors.Add(ProcessorType.Bing, new ProcessorInfo()  );
            Processors.Add(ProcessorType.Google, new ProcessorInfo()  );

            APIKeys.Add(ProcessorType.Bing,new APIKey{Type=ProcessorType.Bing, Value="Bing API key..."}  );
            APIKeys.Add(ProcessorType.Google,new APIKey{Type=ProcessorType.Google, Value="Google API key..."}  );
        }

        public InputFileInfo InputFile { get; } = new();
        public OutputFileInfo OutputFile { get; } = new();

        public ProcessorType ProcessorType { get; set; } = ProcessorType.Undefined;

        public Dictionary<ProcessorType, ProcessorInfo> Processors { get; set; } = new();

        public ProcessorInfo ProcessorInfo => Processors.ContainsKey( ProcessorType )
            ? Processors[ ProcessorType ]
            : new ProcessorInfo();

        public Dictionary<ProcessorType, APIKey> APIKeys { get; set; } = new();

        public string APIKey
        {
            get => APIKeys.ContainsKey( ProcessorType ) ? APIKeys[ ProcessorType ].Value : string.Empty;

            set
            {
                if( APIKeys.ContainsKey( ProcessorType ) )
                    APIKeys[ ProcessorType ].Value = value;
                else APIKeys.Add( ProcessorType, new APIKey { Value = value, Type = ProcessorType } );
            }
        }

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