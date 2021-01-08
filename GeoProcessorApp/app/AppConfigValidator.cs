using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Alba.CsConsoleFormat;
using Alba.CsConsoleFormat.Fluent;
using J4JSoftware.Logging;

namespace J4JSoftware.GeoProcessor
{
    public class AppConfigValidator : IAppConfigValidator
    {
        public void Validate( AppConfig config )
        {
            ValidateProcessorType( config );
            ValidateAPIKey( config );
            ValidateInputFile( config );
        }

        private void ValidateProcessorType( AppConfig config )
        {
            if( !config.RunInteractive && config.ProcessorType != ProcessorType.Undefined )
                return;

            Console.WriteLine();
            Colors.WriteLine( "\nProcessorType".Yellow(), " is undefined\n" );

            config.ProcessorType = ConsoleExtensions.GetEnum<ProcessorType>(
                config.ProcessorType,
                ProcessorType.Google,
                Enum.GetValues<ProcessorType>()
                    .Where( x => x.IsSecuredProcessor() )
                    .ToList() );
        }

        private void ValidateAPIKey( AppConfig config )
        {
            if( !config.RunInteractive && !string.IsNullOrEmpty( config.APIKey ) )
                return;

            Console.WriteLine();
            config.APIKey = ConsoleExtensions.GetText( config.APIKey, $"{config.ProcessorType} APIKey" );
        }

        private void ValidateInputFile( AppConfig config )
        {
            if( !config.RunInteractive && !string.IsNullOrEmpty( config.InputFile.GetPath() ) )
                return;

            Console.WriteLine();
            var filePath = ConsoleExtensions.GetText( config.InputFile.GetPath(),
                string.Empty,
                "full path to input file ".Yellow() );

            if( string.IsNullOrEmpty( filePath ) )
                return;

            config.InputFile.FilePath = filePath;

            if( config.InputFile.Type == ImportType.Unknown )
                config.InputFile.FilePath = string.Empty;
        }
    }
}
