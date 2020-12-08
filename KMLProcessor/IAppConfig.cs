namespace J4JSoftware.KMLProcessor
{
    public interface IAppConfig
    {
        double CoalesceValue { get; }
        UnitTypes CoalesceUnit { get; }
        double MaxBearingStdDev { get; }
        string InputFile { get; }
        string OutputFile { get; }
        bool IsValid( out string? error );
    }
}