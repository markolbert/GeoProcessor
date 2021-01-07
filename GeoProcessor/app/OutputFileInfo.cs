using System;

namespace J4JSoftware.GeoProcessor
{
    public class OutputFileInfo : FileInfo<ExportType>
    {
        protected override ExportType GetTypeFromExtension(string? ext)
        {
            if( ext?.Length > 0
                && Enum.TryParse( typeof(ExportType), ext, true, out var parsed ) )
                return (ExportType) parsed!;

            return ExportType.KML;
        }

        protected override string GetExtensionFromType(ExportType type) =>
            type switch
            {
                ExportType.KMZ => ".kmz",
                _ => ".kml"
            };
    }
}