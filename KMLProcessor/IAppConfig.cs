namespace J4JSoftware.KMLProcessor
{
    public interface IAppConfig
    {
        string KmlFile { get; }
        string OutputFolder { get; }
        bool IsValid( out string? error );
    }
}