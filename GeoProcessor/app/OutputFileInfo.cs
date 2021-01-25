using System;

namespace J4JSoftware.GeoProcessor
{
    public class OutputFileInfo : FileInfo<ExportType>
    {
        protected override ExportType GetTypeFromExtension(string? ext)
        {
            if( ext?.Length > 0
                && Enum.TryParse( typeof(ExportType), ext[1..], true, out var parsed ) )
                return (ExportType) parsed!;

            return ExportType.Unknown;
        }

        protected override string GetExtensionFromType( ExportType type ) =>
            type switch
            {
                ExportType.KMZ => ".kmz",
                ExportType.KML => ".kml",
                _ => string.Empty
            };
    }
}