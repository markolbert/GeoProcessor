using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace J4JSoftware.KMLProcessor
{
    public interface IAppConfig
    {
        SnapProcessorType SnapProcessorType { get; }
        List<SnapProcessorAPIKey> APIKeys { get; }

        [JsonIgnore]
        bool StoreAPIKey { get; }

        CoalesenceType CoalesenceType { get; }
        double CoalesceValue { get; }
        UnitTypes CoalesceUnit { get; }
        Distance CoalesenceDistance { get; }
        double MaxBearingDelta { get; }

        [JsonIgnore]
        string? InputFile { get; }
        bool ZipOutputFile { get; }
        string? OutputFile { get; }

        bool IsValid( out string? error );
    }
}