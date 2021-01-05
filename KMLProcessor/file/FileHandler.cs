using System.IO;
using System.Text;
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

        public string GetNumberedFilePath( int idx )
        {
            var fileDir = Path.GetDirectoryName( FilePath );
            var fileNameNoExt = Path.GetFileNameWithoutExtension( FilePath );
            var fileExt = Path.GetExtension( FilePath );

            var sb = new StringBuilder();

            sb.Append( fileNameNoExt );

            if( idx > 0 )
                sb.Append( $"-{idx + 1}" );

            sb.Append( fileExt );

            return Path.Combine( fileDir ?? string.Empty, sb.ToString() );
        }
    }
}