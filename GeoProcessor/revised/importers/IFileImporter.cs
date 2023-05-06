namespace J4JSoftware.GeoProcessor;

public interface IFileImporter : IImporter2
{
    string FileType { get; }
    bool LineStringsOnly { get; set; }
}
