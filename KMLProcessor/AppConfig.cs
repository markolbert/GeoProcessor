using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace J4JSoftware.KMLProcessor
{
    public class AppConfig : IAppConfig
    {
        public static readonly Dictionary<string, string> CommandLineMappings =
            new Dictionary<string, string>
            {
                { "-i", "Configuration:InputFile" },
                { "--inputFile", "Configuration:InputFile" },
                { "-z", "Configuration:ZipOutputFile" },
                { "--zipOutput", "Configuration:ZipOutputFile" },
                { "-d", "Configuration:CoalesceValue" },
                { "--minDistanceValue", "Configuration:CoalesceValue" },
                { "-u", "Configuration:CoalesceUnit" },
                { "--minDistanceUnit", "Configuration:CoalesceUnit" },
                { "-k", "Configuration:APIKey" },
                { "--apiKey", "Configuration:APIKey" },
                { "-p", "Configuration:SnapProcessorType" },
                { "--snapProcessor", "Configuration:SnapProcessorType" },
                { "--s", "Configuration:StoreAPIKey" },
                { "--storeAPIKey", "Configuration:StoreAPIKey" },
            };

        public SnapProcessorType SnapProcessorType { get; set; } = SnapProcessorType.Bing;
        public List<SnapProcessorAPIKey> APIKeys { get; set; } 
        public bool StoreAPIKey { get; set; }

        public CoalesenceTypes CoalesenceTypes { get; set; } = CoalesenceTypes.Distance;
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

            // nothing to check if we're just storing an API key
            if( StoreAPIKey )
                return true;

            if( !File.Exists( InputFile ) )
            {
                error = $"Source file '{InputFile}' is not accessible";
                return false;
            }

            if( string.IsNullOrEmpty( APIKeys.FirstOrDefault( k => k.Type == SnapProcessorType )?.APIKey ) )
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

        [ System.Diagnostics.CodeAnalysis.SuppressMessage( 
            "Interoperability", 
            "CA1416:Validate platform compatibility",
            Justification = "<Pending>" ) ]
        public bool Encrypt( string data, out string? result )
        {
            result = null;

            var byteData = Encoding.UTF8.GetBytes( data );

            try
            {
                var encrypted = ProtectedData.Protect( byteData, null, scope : DataProtectionScope.CurrentUser );
                result = Encoding.UTF8.GetString( encrypted );

                return true;
            }
            catch
            {
                return false;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Interoperability",
            "CA1416:Validate platform compatibility",
            Justification = "<Pending>")]
        public bool Decrypt(string data, out string? result)
        {
            result = null;

            var byteData = Encoding.UTF8.GetBytes(data);

            try
            {
                var decrypted = ProtectedData.Unprotect(byteData, null, scope: DataProtectionScope.CurrentUser);
                result = Encoding.UTF8.GetString(decrypted);

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}