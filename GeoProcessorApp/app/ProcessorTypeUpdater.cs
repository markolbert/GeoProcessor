#region license

// Copyright 2021 Mark A. Olbert
// 
// This library or program 'GeoProcessorApp' is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public License as
// published by the Free Software Foundation, either version 3 of the License,
// or (at your option) any later version.
// 
// This library or program is distributed in the hope that it will be useful, but
// WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// General Public License for more details.
// 
// You should have received a copy of the GNU General Public License along with
// this library or program.  If not, see <https://www.gnu.org/licenses/>.

#endregion

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

            newValue = Prompters.GetEnum(
                origValue,
                ProcessorType.Google,
                Enum.GetValues<ProcessorType>()
                    .Where( x => x.SnapsToRoute() )
                    .ToList() );

            return UpdaterResult.Changed;
        }
    }
}