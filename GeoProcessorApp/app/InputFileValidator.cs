using System;
using System.IO;
using System.Linq;
using Alba.CsConsoleFormat.Fluent;
using J4JSoftware.ConsoleUtilities;
using J4JSoftware.Logging;

namespace J4JSoftware.GeoProcessor
{
    public class InputFileValidator : PropertyUpdater<InputFileInfo>
    {
        public InputFileValidator( IJ4JLogger? logger )
            : base( logger )
        {
        }

        public override UpdaterResult Validate( InputFileInfo? origValue, out InputFileInfo? newValue )
        {
            newValue = origValue ?? new InputFileInfo();

            if( !string.IsNullOrEmpty( origValue?.GetPath() ) && File.Exists( origValue?.GetPath() ) )
                return UpdaterResult.OriginalOkay;

            Console.WriteLine();
            var filePath = ConsoleExtensions.GetText( origValue?.GetPath() ?? "**undefined**",
                string.Empty,
                "full path to input file ".Yellow() );

            if( string.IsNullOrEmpty( filePath ) )
                return UpdaterResult.InvalidUserInput;

            newValue.FilePath = filePath;

            if( newValue.Type != ImportType.Unknown )
                return UpdaterResult.Changed;

            newValue.FilePath = string.Empty;

            return UpdaterResult.InvalidUserInput;
        }
    }
}