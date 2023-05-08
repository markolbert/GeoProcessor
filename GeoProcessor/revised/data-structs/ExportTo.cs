namespace J4JSoftware.GeoProcessor.RouteBuilder;

public record ExportToBase(IExporter2 Exporter);

public record ExportTo( IExporter2 Exporter, IExportTarget Target ) : ExportToBase( Exporter );

public record ExportToFile(string FilePath, IExporter2 Exporter) : ExportToBase(Exporter);
