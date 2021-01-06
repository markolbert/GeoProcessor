using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Alba.CsConsoleFormat;
using Alba.CsConsoleFormat.Fluent;
using J4JSoftware.Logging;

namespace J4JSoftware.KMLProcessor
{
    public class AppConfigValidator : IAppConfigValidator
    {
        private const string Indent = "    ";

        private readonly IJ4JLogger _logger;

        public AppConfigValidator( IJ4JLogger logger )
        {
            _logger = logger;
            _logger.SetLoggedType( GetType() );
        }

        public void Validate( AppConfig config )
        {
            ValidateProcessorType( config );
            ValidateAPIKey( config );
            ValidateInputFile(config); ;
        }

        private void ValidateProcessorType( AppConfig config )
        {
            if( config.ProcessorType != ProcessorType.Undefined )
                return;

            Colors.WriteLine( "\nProcessorType".Yellow(), " is undefined\n" );

            config.ProcessorType = GetEnum<ProcessorType>( 
                ProcessorType.Google,
                Enum.GetValues<ProcessorType>()
                    .Where( x => x.IsSecuredProcessor() )
                    .ToList() );
        }

        private void ValidateAPIKey( AppConfig config )
        {
            if( !string.IsNullOrEmpty( config.APIKey ) )
                return;

            config.APIKey = GetText( $"\n{config.ProcessorType} APIKey", string.Empty );
        }

        private void ValidateInputFile( AppConfig config )
        {
            if( !string.IsNullOrEmpty( config.InputFile ) )
                return;

            var filePath = GetText( string.Empty,
                "input file ".Yellow(),
                "(full path)".Green() );

            if( string.IsNullOrEmpty( filePath ) )
                return;

            config.InputFile = filePath;

            if (config.InputFileDetails.Type == ImportType.Unknown)
                config.InputFile = string.Empty;
        }

        private T GetEnum<T>( T defaultValue, List<T>? values = null )
            where T : Enum
        {
            Colors.WriteLine("Enter ", typeof(T).Name.Yellow(), ":\n");

            values ??= Enum.GetValues( typeof(T) ).Cast<T>().ToList();

            for( var idx = 0; idx < values.Count; idx++ )
            {
                Colors.WriteLine( Indent, 
                    ( idx + 1 ).ToString().Green(), 
                    " - ",
                    values[ idx ].ToString() );
            }

            Console.Write("\n\nChoice: ");

            var text = Console.ReadLine();

            if( !string.IsNullOrEmpty( text ) 
                && int.TryParse( text, NumberStyles.Integer, null, out var choice ) 
                && choice >= 1 
                && choice <= values.Count ) 
                return values[ choice - 1 ];

            _logger.Error<string, string>( "Invalid response '{0}', returning default '{1}'",
                text ?? string.Empty,
                defaultValue.ToString() );

            return defaultValue;
        }

        private string GetText( string prompt, string defaultValue )
        {
            Colors.WriteLine("\nEnter ", prompt.Green(), ": ");
            Console.Write("> "  );

            var retVal = Console.ReadLine();

            return string.IsNullOrEmpty(retVal) ? defaultValue : retVal;
        }

        private string GetText(string defaultValue, params Span[] prompts )
        {
            Colors.WriteLine("\nEnter ", prompts, ": ");
            Console.Write("> ");

            var retVal = Console.ReadLine();

            return string.IsNullOrEmpty(retVal) ? defaultValue : retVal;
        }
    }
}
