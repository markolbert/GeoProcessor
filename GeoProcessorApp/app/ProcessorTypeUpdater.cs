using System;
using System.Linq;
using Alba.CsConsoleFormat.Fluent;
using J4JSoftware.ConsoleUtilities;
using J4JSoftware.Logging;

namespace J4JSoftware.GeoProcessor
{
    public class ProcessorTypeUpdater : PropertyUpdater<ProcessorType>
    {
        public ProcessorTypeUpdater( IJ4JLogger? logger )
            : base( logger )
        {
        }

        public override UpdaterResult Update( ProcessorType origValue, out ProcessorType newValue )
        {
            newValue = origValue;

            if( origValue != ProcessorType.None )
                return UpdaterResult.OriginalOkay;

            Console.WriteLine();
            Colors.WriteLine( "\nProcessorType".Yellow(), " is undefined\n" );

            newValue = Prompters.GetEnum<ProcessorType>(
                origValue,
                ProcessorType.Google,
                Enum.GetValues<ProcessorType>()
                    .Where( x => x.SnapsToRoute() )
                    .ToList() );

            return UpdaterResult.Changed;
        }
    }
}