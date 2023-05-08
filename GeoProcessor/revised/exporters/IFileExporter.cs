namespace J4JSoftware.GeoProcessor;

public interface IFileExporter : IExporter2
{
    string FileType { get; }
    string FilePath { get; }
}
