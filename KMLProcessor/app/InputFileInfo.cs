using System;

namespace J4JSoftware.KMLProcessor
{
    public class InputFileInfo : FileInfo<ImportType>
    {
        protected override ImportType GetTypeFromExtension( string? ext )
        {
            if( ext?.Length > 0
                && Enum.TryParse( typeof(ImportType), ext[1..], true, out var parsed ) )
                return (ImportType) parsed!;

            return ImportType.Unknown;
        }

        protected override string GetExtensionFromType( ImportType type ) =>
            type switch
            {
                ImportType.GPX => ".gpx",
                ImportType.KML => ".kml",
                ImportType.KMZ => ".kmz",
                _ => string.Empty
            };
    }
}