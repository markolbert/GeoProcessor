using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using J4JSoftware.Logging;

namespace J4JSoftware.KMLProcessor
{
    public class AppConfig
    {
        private string? _outputFile = null;
        private ExportType _exportType = ExportType.KML;

        public ProcessorType ProcessorType { get; set; } = ProcessorType.Undefined;
        public Dictionary<ProcessorType, ProcessorInfo>? Processors { get; set; }
        public Dictionary<ProcessorType, APIKey>? APIKeys { get; set; }
        
        public bool StoreAPIKey { get; set; }
        public string? InputFile { get; set; }

        public string DefaultRouteName { get; set; } = "Unnamed Route";
        public int RouteWidth { get; set; } = 4;
        public Color RouteColor { get; set; } = Color.Red;
        public Color RouteHighlightColor { get; set; } = Color.DarkTurquoise;

        public ExportType ExportType
        {
            get => _exportType;

            set
            {
                _exportType = value;

                if( _outputFile == null )
                    return;

                // adjust the output file name
                var dir = Path.GetDirectoryName(_outputFile);
                var noExt = Path.GetFileNameWithoutExtension(_outputFile);

                _outputFile = Path.Combine( dir!, $"{noExt}{( _exportType == ExportType.KMZ ? ".kmz" : ".kml" )}" );
            }
        }

        public string? OutputFile
        {
            get
            {
                if( _outputFile != null ) 
                    return _outputFile;

                if( InputFile == null )
                    return _outputFile;

                var dir = Path.GetDirectoryName( InputFile );
                var noExt = Path.GetFileNameWithoutExtension( InputFile );

                _outputFile = Path.Combine( dir!,
                    $"{noExt}-processed{( ExportType == ExportType.KMZ ? ".kmz" : ".kml" )}" );

                return _outputFile;
            }

            set
            {
                // adjust the _exportType to be consistent with the file name we were given
                var ext = Path.GetExtension(value);

                var priorExportType = _exportType;

                if( ext?.Length >=1 
                    && Enum.TryParse( typeof(ExportType), ext[1..], true, out var valueExportType ) )
                {
                    _exportType = (ExportType) valueExportType!;

                    if( priorExportType == _exportType )
                    {
                        _outputFile = value;
                        return;
                    }
                }

                var dir = Path.GetDirectoryName(value);
                var noExt = Path.GetFileNameWithoutExtension(value);

                _outputFile = Path.Combine( dir!, $"{noExt}{( ExportType == ExportType.KMZ ? ".kmz" : ".kml" )}" );
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