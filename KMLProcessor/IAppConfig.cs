namespace J4JSoftware.KMLProcessor
{
    public interface IAppConfig
    {
        string InputFile { get; }
        string OutputFile { get; }
        bool IsValid( out string? error );
    }
}