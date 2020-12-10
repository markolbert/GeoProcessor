namespace J4JSoftware.KMLProcessor
{
    public interface IAppConfig
    {
        string BingMapsKey { get; }
        double CoalesceValue { get; }
        UnitTypes CoalesceUnit { get; }
        Distance CoalesenceDistance { get; }
        CoalesenceTypes CoalesenceTypes { get; }
        double MaxBearingDelta { get; }
        string InputFile { get; }
        string OutputFile { get; }
        bool IsValid( out string? error );
    }
}