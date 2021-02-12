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
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using J4JSoftware.Logging;
using Microsoft.Extensions.Hosting;

namespace J4JSoftware.GeoProcessor
{
    internal class Program
    {
        public const string AppName = "GeoProcessor";
        public const string AppConfigFile = "appConfig.json";
        public const string UserConfigFile = "userConfig.json";

        public static string AppUserFolder = Path.Combine(
            Environment.GetFolderPath( Environment.SpecialFolder.LocalApplicationData ),
            "J4JSoftware",
            AppName );

        private static readonly J4JCachedLogger _cachedLogger = new();
        private static readonly CancellationToken _cancellationToken = new();

        private static async Task Main( string[] args )
        {
            if( !CompositionRoot.Default.Initialized )
            {
                Console.WriteLine( $"{nameof(CompositionRoot)} failed to initialize" );
                Environment.ExitCode = -1;

                return;
            }

            await CompositionRoot.Default.Host!.RunAsync( _cancellationToken );
        }
    }
}