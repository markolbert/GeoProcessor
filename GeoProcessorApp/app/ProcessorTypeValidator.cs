using System;
using System.Linq;
using Alba.CsConsoleFormat.Fluent;
using J4JSoftware.ConsoleUtilities;
using J4JSoftware.Logging;

namespace J4JSoftware.GeoProcessor
{
    public class ProcessorTypeValidator : PropertyUpdater<ProcessorType>
    {
        public ProcessorTypeValidator( IJ4JLogger? logger )
            : base( logger )
        {
        }

        public override UpdaterResult Validate( ProcessorType origValue, out ProcessorType newValue )
        {
            newValue = origValue;

            if( origValue != ProcessorType.Undefined )
                return UpdaterResult.OriginalOkay;

            Console.WriteLine();
            Colors.WriteLine( "\nProcessorType".Yellow(), " is undefined\n" );

            newValue = GetEnum<ProcessorType>(
                origValue,
                ProcessorType.Google,
                Enum.GetValues<ProcessorType>()
                    .Where( x => GeoExtensions.IsSecuredProcessor( x ) )
                    .ToList() );

            return UpdaterResult.Changed;
        }
    }
}