using System.Collections.Generic;
using System.Drawing;
using System.IO;
using J4JSoftware.ConsoleUtilities;
using J4JSoftware.Logging;

namespace J4JSoftware.GeoProcessor
{
    public class AppConfig : IImportConfig, IExportConfig
    {
        public bool StoreAPIKey { get; set; }
        public bool RunInteractive { get; set; }

        public InputFileInfo InputFile{ get; } = new();

        public ExportType ExportType
        {
            get => OutputFile.Type;
            set => OutputFile.Type = value;
        }
        public OutputFileInfo OutputFile { get; } = new();

        public ProcessorType ProcessorType { get; set; } = ProcessorType.Undefined;
        public Dictionary<ProcessorType, ProcessorInfo> Processors { get; set; } =
            new Dictionary<ProcessorType, ProcessorInfo>();
        public ProcessorInfo ProcessorInfo => Processors.ContainsKey( ProcessorType )
            ? Processors[ ProcessorType ]
            : new ProcessorInfo();

        public Dictionary<ProcessorType, APIKey> APIKeys { get; set; } = new Dictionary<ProcessorType, APIKey>();
        
        public string? APIKey
        {
            get => APIKeys.ContainsKey( ProcessorType ) ? APIKeys[ ProcessorType ].Value : null;

            set
            {
                if( value == null )
                    return;

                if( APIKeys.ContainsKey( ProcessorType ) )
                    APIKeys[ ProcessorType ].Value = value;
                else APIKeys.Add( ProcessorType, new APIKey { Value = value, Type = ProcessorType } );
            }
        }

        public string DefaultRouteName { get; set; } = "Unnamed Route";
        public int RouteWidth { get; set; } = 4;
        public Color RouteColor { get; set; } = Color.Red;
        public Color RouteHighlightColor { get; set; } = Color.DarkTurquoise;

        public J4JLoggerConfiguration? Logging { get; set; }

        public bool IsValid( IJ4JLogger? logger )
        {
            // if we're storing an API key there's nothing to check
            if( StoreAPIKey )
                return true;

            var filePath = InputFile.GetPath();

            if( !File.Exists( filePath ) )
            {
                logger?.Error<string>( "Source file '{0}' is not accessible", filePath );
                return false;
            }

            if( Processors?.Count == 0 )
            {
                logger?.Error( "No processors are defined" );
                return false;
            }

            if( !Processors!.ContainsKey( ProcessorType ) )
            {
                logger?.Error( "No {0} processor is defined", ProcessorType );
                return false;
            }

            if( string.IsNullOrEmpty( APIKey ) )
            {
                logger?.Error( "API key for {0} is not defined", ProcessorType );
                return false;
            }

            return true;
        }
    }
}