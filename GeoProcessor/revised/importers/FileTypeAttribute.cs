using System;

namespace J4JSoftware.GeoProcessor;

[AttributeUsage(AttributeTargets.Class, Inherited=false)]
public class FileTypeAttribute : Attribute
{
    public FileTypeAttribute(
        string fileType
    )
    {
        FileType = fileType;
    }

    public string FileType { get; }
}