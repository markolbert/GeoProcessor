using J4JSoftware.Logging;

namespace J4JSoftware.KMLProcessor
{
    public class FileHandler
    {
        protected FileHandler( AppConfig config, IJ4JLogger logger )
        {
            Configuration = config;

            Logger = logger;
            Logger.SetLoggedType( GetType() );
        }

        protected IJ4JLogger Logger { get; }
        protected AppConfig Configuration { get; }

        public string FilePath { get; protected set; } = string.Empty;
    }
}