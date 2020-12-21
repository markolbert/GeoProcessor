using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;

namespace J4JSoftware.KMLProcessor
{
    public class AppConfig : IAppConfig
    {
        public SnapProcessorType SnapProcessorType { get; set; } = SnapProcessorType.Undefined;
        public List<SnapProcessorAPIKey> APIKeys { get; set; } = new List<SnapProcessorAPIKey>();
        
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

        public bool IsValid( out string? error )
        {
            error = null;

            // if we're storing an API key we must have a SnapProcessorType defined
            if( StoreAPIKey )
            {
                if( SnapProcessorType != SnapProcessorType.Undefined )
                    return true;
                
                error = "Undefined SnapProcessorType";
                return false;
            }

            if( !File.Exists( InputFile ) )
            {
                error = $"Source file '{InputFile}' is not accessible";
                return false;
            }

            if( string.IsNullOrEmpty( APIKeys.FirstOrDefault( k => k.Type == SnapProcessorType )?.EncryptedAPIKey ) )
            {
                error = "API key is not defined";
                return false;
            }

            error = SnapProcessorType switch
            {
                SnapProcessorType.Google => null,
                SnapProcessorType.Bing => null,
                _ => "Unsupported snap to route processor"
            };

            return error != null;
        }
    }
}