using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using J4JSoftware.GeoProcessor;

namespace Test.GeoProcessor
{
    public class GeoConfig : IExportConfig, IImportConfig
    {
        public class APIKeyValue
        {
            public string Value { get; set; }
        }

        public ProcessorType ProcessorType { get; set; } = ProcessorType.Google;

        public ProcessorInfo ProcessorInfo { get; } = new()
        {
            MaxSeparation = new Distance( UnitTypes.km, 2 ), 
            MaxDistanceMultiplier = 3, 
            MaxPointsPerRequest = 100
        };

        public OutputFileInfo OutputFile { get; } = new OutputFileInfo();
        public int RouteWidth { get; set; } = 4;
        public Color RouteColor { get; set; } = Color.Red;
        public Color RouteHighlightColor { get; set; } = Color.DarkTurquoise;
        public Dictionary<ProcessorType, APIKeyValue> APIKeys { get; set; }
        public string APIKey
        {
            get => APIKeys.ContainsKey( ProcessorType ) ? APIKeys[ ProcessorType ].Value : string.Empty;

            set
            {
                if( APIKeys.TryGetValue( ProcessorType, out var apiKey ) )
                    apiKey.Value = value;
            }
        }
    }
}
