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
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "J4JSoftware",
            AppName);

        private static readonly J4JCachedLogger _cachedLogger = new();
        private static readonly CancellationToken _cancellationToken = new();

        private static async Task Main( string[] args )
        {
            if( !CompositionRoot.Default.Initialized )
            {
                Console.WriteLine($"{nameof(CompositionRoot)} failed to initialize");
                Environment.ExitCode = -1;

                return;
            }

            await CompositionRoot.Default.Host!.RunAsync( _cancellationToken );
        }
    }
}