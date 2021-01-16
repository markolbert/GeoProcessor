using System;
using System.Linq;
using Alba.CsConsoleFormat.Fluent;
using J4JSoftware.ConsoleUtilities;
using J4JSoftware.Logging;

namespace J4JSoftware.GeoProcessor
{
    public class ApiKeyValidator : PropertyUpdater<string>
    {
        public ApiKeyValidator( IJ4JLogger? logger )
            : base( logger )
        {
        }

        public override UpdaterResult Validate( string? origValue, out string? newValue )
        {
            newValue = origValue;

            if( !string.IsNullOrEmpty( origValue ) )
                return UpdaterResult.OriginalOkay;

            Console.WriteLine();
            newValue = GetText( origValue ?? "**undefined**", "APIKey" );

            return UpdaterResult.Changed;
        }
    }
}