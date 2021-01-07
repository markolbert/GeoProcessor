using System.Collections.Generic;
using System.Drawing;
using System.IO;
using J4JSoftware.Logging;

namespace J4JSoftware.KMLProcessor
{
    public class AppConfig
    {
        private string? _apiKey;

        internal InputFileInfo InputFileDetails { get; } = new();
        internal OutputFileInfo OutputFileDetails { get; } = new();

        public ProcessorType ProcessorType { get; set; } = ProcessorType.Undefined;
        public Dictionary<ProcessorType, ProcessorInfo>? Processors { get; set; }
        public Dictionary<ProcessorType, APIKey>? APIKeys { get; set; }

        public string? APIKey
        {
            get
            {
                if( string.IsNullOrEmpty( _apiKey )
                    && ( APIKeys?.ContainsKey( ProcessorType ) ?? false ) )
                    return APIKeys![ ProcessorType ].Value;

                return _apiKey;
            }

            set => _apiKey = value;
        }
        
        public bool StoreAPIKey { get; set; }

        public string InputFile
        {
            get => InputFileDetails.FilePath;
            set => InputFileDetails.FilePath = value;
        }

        public string DefaultRouteName { get; set; } = "Unnamed Route";
        public int RouteWidth { get; set; } = 4;
        public Color RouteColor { get; set; } = Color.Red;
        public Color RouteHighlightColor { get; set; } = Color.DarkTurquoise;

        public ExportType ExportType
        {
            get => OutputFileDetails.Type;
            set => OutputFileDetails.Type = value;
        }

        public string OutputFile
        {
            get => OutputFileDetails.FilePath;
            set => OutputFileDetails.FilePath = value;
        }

        public J4JLoggerConfiguration? Logging { get; set; }

        public bool IsValid( IJ4JLogger? logger )
        {
            // if we're storing an API key there's nothing to check
            if (StoreAPIKey)
                return true;

            if( !File.Exists( InputFile ) )
            {
                logger?.Error<string>( "Source file '{0}' is not accessible", InputFile ?? string.Empty );
                return false;
            }

            if( Processors?.Count == 0 )
            {
                logger?.Error( "No processors are defined" );
                return false;
            }

            if( !Processors!.ContainsKey(ProcessorType ) )
            {
                logger?.Error("No {0} processor is defined", ProcessorType);
                return false;
            }

            if( APIKeys?.Count == 0 )
            {
                logger?.Error("No API keys are defined");
                return false;
            }

            if( !APIKeys!.ContainsKey( ProcessorType ) )
            {
                logger?.Error( "{0} API key is not defined", ProcessorType );
                return false;
            }

            return true;
        }
    }
}