using System;

namespace J4JSoftware.GeoProcessor;

[AttributeUsage(AttributeTargets.Class, Inherited=false)]
public class ImportFileTypeAttribute : Attribute
{
    public ImportFileTypeAttribute(
        string fileType
    )
    {
        FileType = fileType;
    }

    public string FileType { get; }
}