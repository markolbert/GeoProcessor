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
        public SnapProcessorType SnapProcessorType { get; set; } = SnapProcessorType.Undefined;
        public List<SnapProcessorAPIKey> APIKeys { get; set; }
        
        public bool StoreAPIKey { get; set; }

        public CoalesenceType CoalesenceType { get; set; } = CoalesenceType.Distance;
        public double CoalesceValue { get; set; } = 10;
        public UnitTypes CoalesceUnit { get; set; } = UnitTypes.Feet;
        public Distance CoalesenceDistance => new Distance( CoalesceUnit, CoalesceValue );
        public double MaxBearingDelta { get; set; } = 5.0;

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

        public J4JLoggerConfiguration Logging { get; set; }

        public bool IsValid( IJ4JLogger? logger )
        {
            // if we're storing an API key we must have a SnapProcessorType defined
            if( StoreAPIKey )
            {
                if( SnapProcessorType != SnapProcessorType.Undefined )
                    return true;

                logger?.Error( "Undefined SnapProcessorType" );
                return false;
            }

            if( !File.Exists( InputFile ) )
            {
                logger?.Error<string>( "Source file '{)}' is not accessible", InputFile );
                return false;
            }

            if( !APIKeys.Any( k => k.Type == SnapProcessorType && !string.IsNullOrEmpty(k.APIKey) ) )
            {
                logger?.Error( "{0} API key is not defined", SnapProcessorType );
                return false;
            }

            return true;
        }
    }
}