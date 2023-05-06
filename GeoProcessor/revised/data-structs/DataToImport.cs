using System.Collections.Generic;

namespace J4JSoftware.GeoProcessor.RouteBuilder;

public record DataToImportBase(
    IImporter2 Importer,
    double MinPointGap,
    double MinOverallGap
);

public record DataToImport(
    string Name,
    IEnumerable<Coordinate2> Coordinates,
    IImporter2 Importer,
    double MinPointGap,
    double MaxDistanceFromStart
) : DataToImportBase( Importer, MinPointGap, MaxDistanceFromStart );

public record FileToImport(
    string FilePath,
    IImporter2 Importer,
    double MinPointGap,
    double MaxDistanceFromStart
) : DataToImportBase(Importer, MinPointGap, MaxDistanceFromStart);
