using System;
using System.Linq;
using Alba.CsConsoleFormat.Fluent;
using J4JSoftware.ConsoleUtilities;
using J4JSoftware.Logging;

namespace J4JSoftware.GeoProcessor
{
    public class APIKeyUpdater : PropertyUpdater<string>
    {
        public APIKeyUpdater( IJ4JLogger? logger )
            : base( logger )
        {
        }

        public override UpdaterResult Validate( string? origValue, out string? newValue )
        {
            newValue = origValue;

            if( !string.IsNullOrEmpty( origValue ) )
                return UpdaterResult.OriginalOkay;

            Console.WriteLine();
            newValue = GetSingleValue( origValue ?? "**undefined**", "APIKey" );

            return UpdaterResult.Changed;
        }
    }
}