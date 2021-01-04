using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;
using J4JSoftware.Logging;

namespace J4JSoftware.KMLProcessor
{
    public class AppConfig
    {
        public ProcessorType ProcessorType { get; set; } = ProcessorType.Undefined;
        public Dictionary<ProcessorType, ProcessorInfo>? Processors { get; set; }
        public Dictionary<ProcessorType, APIKey>? APIKeys { get; set; }
        
        public bool StoreAPIKey { get; set; }
        public string? InputFile { get; set; }
        public bool ZipOutputFile { get; set; }

        public string? OutputFile
        {
            get
            {
                if( string.IsNullOrEmpty( InputFile ) )
                    return null;

                var dir = Path.GetDirectoryName( InputFile );
                var noExt = Path.GetFileNameWithoutExtension( InputFile );

                return Path.Combine( dir!, $"{noExt}-processed{( ZipOutputFile ? ".kmz" : ".kml" )}" );
            }
        }

        public J4JLoggerConfiguration? Logging { get; set; }

        public bool IsValid( IJ4JLogger? logger )
        {
            // if we're storing an API key we must have a SnapProcessorType defined
            if( StoreAPIKey )
            {
                if( ProcessorType != ProcessorType.Undefined )
                    return true;

                logger?.Error( "Undefined SnapProcessorType" );
                return false;
            }

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