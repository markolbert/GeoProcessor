﻿using System;
using System.IO;
using System.Linq;
using Alba.CsConsoleFormat.Fluent;
using J4JSoftware.ConsoleUtilities;
using J4JSoftware.Logging;

namespace J4JSoftware.GeoProcessor
{
    public class InputFileUpdater : PropertyUpdater<InputFileInfo>
    {
        public InputFileUpdater( IJ4JLogger? logger )
            : base( logger )
        {
        }

        public override UpdaterResult Update( InputFileInfo? origValue, out InputFileInfo? newValue )
        {
            newValue = origValue ?? new InputFileInfo();

            if( !string.IsNullOrEmpty( origValue?.GetPath() ) && File.Exists( origValue?.GetPath() ) )
                return UpdaterResult.OriginalOkay;

            Console.WriteLine();
            var filePath = Prompters.GetSingleValue( origValue?.GetPath() ?? "**undefined**",
                string.Empty,
                "full path to input file " );

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