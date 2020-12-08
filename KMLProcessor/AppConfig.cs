using System;
using System.IO;

namespace J4JSoftware.KMLProcessor
{
    public class AppConfig : IAppConfig
    {
        private string? _outFile;

        public double CoalesceValue { get; set; } = 10;
        public UnitTypes CoalesceUnit { get; set; } = UnitTypes.Feet;

        public double MaxBearingStdDev { get; set; } = 2.0;

        public string InputFile { get; set; } = string.Empty;

        public string OutputFile
        {
            get
            {
                if( string.IsNullOrEmpty( _outFile ) && !string.IsNullOrEmpty( InputFile ) )
                {
                    var dir = Path.GetDirectoryName( InputFile );
                    var noExt = Path.GetFileNameWithoutExtension( InputFile );

                    _outFile = Path.Combine( dir!, $"{noExt}-processed{Path.GetExtension( InputFile )}" );
                }

                return _outFile ?? string.Empty;
            }

            set => _outFile = value;
        }

        public bool IsValid( out string? error )
        {
            if( !File.Exists( InputFile ) )
            {
                error = $"Source file '{InputFile}' is not accessible";
                return false;
            }

            error = null;

            return true;
        }
    }
}