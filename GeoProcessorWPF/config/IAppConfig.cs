using System.Collections.Generic;
using J4JSoftware.Logging;

namespace J4JSoftware.GeoProcessor
{
    public interface IAppConfig : IImportConfig, IExportConfig
    {
        void RestoreFrom( CachedAppConfig src );
        string ApplicationConfigurationFolder { get; set; }
        string UserConfigurationFolder { get; set; }
        NetEventConfig? NetEventChannelConfiguration { get; set; }
        InputFileInfo InputFile { get; }
        Dictionary<ProcessorType, ProcessorInfo> Processors { get; set; }
        //Dictionary<ProcessorType, APIKey> APIKeys { get; set; }
        ExportType ExportType { get; set; }
        J4JLoggerConfiguration? Logging { get; set; }
    }
}