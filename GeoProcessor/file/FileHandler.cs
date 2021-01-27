using System;
using J4JSoftware.Logging;

namespace J4JSoftware.GeoProcessor
{
    public class FileHandler
    {
        protected FileHandler( IGeoConfig config, IJ4JLogger? logger )
        {
            Configuration = config;

            Logger = logger;
            Logger?.SetLoggedType( GetType() );
        }

        protected IJ4JLogger? Logger { get; }
        protected IGeoConfig Configuration { get; }
    }
}