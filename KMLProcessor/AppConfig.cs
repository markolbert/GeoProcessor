using System;
using System.IO;

namespace J4JSoftware.KMLProcessor
{
    public class AppConfig : IAppConfig
    {
        public string KmlFile { get; set; } = string.Empty;
        public string OutputFolder { get; set; } = Environment.CurrentDirectory;

        public bool IsValid( out string? error )
        {
            if( !File.Exists( KmlFile ) )
            {
                error = $"Source file '{KmlFile}' is not accessible";
                return false;
            }

            if( !Directory.Exists( OutputFolder ) )
            {
                error = $"Output directory '{OutputFolder}' is not accessible";
                return false;
            }

            error = null;

            return true;
        }
    }
}