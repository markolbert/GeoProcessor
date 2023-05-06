using System.Collections.Generic;

namespace J4JSoftware.GeoProcessor.RouteBuilder;

public record DataToImportBase( IImporter2 Importer );

public record DataToImport(
    string Name,
    IEnumerable<Coordinate2> Coordinates,
    IImporter2 Importer
) : DataToImportBase( Importer );

public record FileToImport(
    string FilePath,
    IImporter2 Importer
) : DataToImportBase(Importer);
