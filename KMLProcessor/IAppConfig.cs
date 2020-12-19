using System.Collections.Generic;

namespace J4JSoftware.KMLProcessor
{
    public interface IAppConfig
    {
        SnapProcessorType SnapProcessorType { get; }
        List<SnapProcessorAPIKey> APIKeys { get; }
        bool StoreAPIKey { get; }

        CoalesenceTypes CoalesenceTypes { get; }
        double CoalesceValue { get; }
        UnitTypes CoalesceUnit { get; }
        Distance CoalesenceDistance { get; }
        double MaxBearingDelta { get; }

        string? InputFile { get; }
        bool ZipOutputFile { get; }
        string? OutputFile { get; }

        bool IsValid( out string? error );

        bool Encrypt( string data, out string? result );
        bool Decrypt( string data, out string? result );
    }
}