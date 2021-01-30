using System.Collections.Generic;
using System.Drawing;

namespace J4JSoftware.GeoProcessor
{
    public class CachedAppConfig
    {
        public CachedAppConfig( IAppConfig src )
        {
            InputPath = src.InputFile.FilePath;
            OutputPath = src.OutputFile.FilePath;
            ProcessorType = src.ProcessorType;

            foreach( var kvp in src.Processors )
            {
                var cachedPI = new ProcessorInfo
                {
                    MaxDistanceMultiplier = kvp.Value.MaxDistanceMultiplier,
                    MaxSeparation = new Distance( kvp.Value.MaxSeparation.Unit, kvp.Value.MaxSeparation.OriginalValue ),
                };

                Processors.Add( kvp.Key, cachedPI );
            }

            APIKey = src.APIKey;
            RouteWidth = src.RouteWidth;
            RouteColor = src.RouteColor;
            RouteHighlightColor = src.RouteHighlightColor;
        }

        public string InputPath { get; }
        public string OutputPath { get; }
        public ProcessorType ProcessorType {get;}
        public Dictionary<ProcessorType, ProcessorInfo> Processors { get; } = new();
        public string APIKey { get; }
        public int RouteWidth { get; }
        public Color RouteColor { get; }
        public Color RouteHighlightColor { get; }
    }
}