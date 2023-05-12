using J4JSoftware.GeoProcessor.RouteBuilder;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.GeoProcessor;

public static class RouteBuilderExtensions
{
    public static RouteBuilder.RouteBuilder AddGpxFile(
        this RouteBuilder.RouteBuilder builder,
        string filePath,
        bool lineStringsOnly = true
    )
    {
        var importer = new GpxImporter2( builder.LoggerFactory ) { LineStringsOnly = lineStringsOnly };

        if( !File.Exists( filePath ) )
            builder.Logger?.LogError( "{filePath} does not exist", filePath );
        else builder.AddDataSource( new FileToImport( filePath, importer ) );

        return builder;
    }

    public static RouteBuilder.RouteBuilder AddCoordinates(
        this RouteBuilder.RouteBuilder builder,
        string collectionName,
        IEnumerable<Coordinate2> coordinates
    )
    {
        if (string.IsNullOrEmpty(collectionName))
            collectionName = "Coordinate Collection";

        builder.AddDataSource( new DataToImport( collectionName,
                                                 coordinates,
                                                 new DataImporter( builder.LoggerFactory ) ) );

        return builder;
    }

    public static RouteBuilder.RouteBuilder SnapWithBing(
        this RouteBuilder.RouteBuilder builder,
        string apiKey,
        int maxPtsPerRequest = 100
    )
    {
        builder.SnapProcessor = new BingProcessor2( maxPtsPerRequest, builder.LoggerFactory ) { ApiKey = apiKey };
        return builder;
    }

    public static RouteBuilder.RouteBuilder SnapWithGoogle(
        this RouteBuilder.RouteBuilder builder,
        string apiKey,
        int maxPtsPerRequest = 100
    )
    {
        builder.SnapProcessor =
            new GoogleProcessor2( maxPtsPerRequest, builder.LoggerFactory ) { ApiKey = apiKey };

        return builder;
    }

    public static RouteBuilder.RouteBuilder ConsolidatePoints(
        this RouteBuilder.RouteBuilder builder,
        Distance2? minPointGap = null,
        Distance2? maxOverallGap = null
    )
    {
        minPointGap ??= new Distance2( UnitType.Meters, GeoConstants.DefaultMinimumPointGapMeters );
        maxOverallGap ??= new Distance2( UnitType.Meters, GeoConstants.DefaultMaximumOverallGapMeters );

        var filter = new ConsolidatePoints( builder.LoggerFactory )
        {
            MaximumOverallGap = maxOverallGap, MinimumPointGap = minPointGap
        };

        builder.AddImportFilter( filter );

        return builder;
    }

    public static RouteBuilder.RouteBuilder ConsolidateAlongBearing(
        this RouteBuilder.RouteBuilder builder,
        double bearingToleranceDegrees = GeoConstants.DefaultBearingToleranceDegrees,
        Distance2? maxConsolDist = null
    )
    {
        // 2.5 km is the max distance between points for Bing
        maxConsolDist ??= new Distance2( UnitType.Kilometers, 2.5 );

        var filter = new ConsolidateAlongBearing( builder.LoggerFactory )
        {
            BearingToleranceDegrees = Math.Abs( bearingToleranceDegrees ),
            MaximumConsolidationDistance = maxConsolDist
        };

        builder.AddImportFilter(filter);

        return builder;
    }

    public static RouteBuilder.RouteBuilder InterpolatePoints(
        this RouteBuilder.RouteBuilder builder,
        Distance2? maxSeparation = null
    )
    {
        maxSeparation ??= new Distance2( UnitType.Kilometers, GeoConstants.DefaultMaxPointSeparationKm );

        var filter = new InterpolatePoints( builder.LoggerFactory ) { MaximumPointSeparation = maxSeparation };

        builder.AddImportFilter( filter );

        return builder;
    }

    public static RouteBuilder.RouteBuilder MergeRoutes(
        this RouteBuilder.RouteBuilder builder,
        Distance2? maxRouteGap = null
    )
    {
        maxRouteGap ??= new Distance2( UnitType.Meters, GeoConstants.DefaultMaxRouteGapMeters );

        var filter = new MergeRoutes( builder.LoggerFactory ) { MaximumRouteGap = maxRouteGap };

        builder.AddImportFilter( filter );

        return builder;
    }

    public static RouteBuilder.RouteBuilder RemoveClusters(
        this RouteBuilder.RouteBuilder builder,
        Distance2? maxClusterDiameter = null
    )
    {
        maxClusterDiameter ??= new Distance2( UnitType.Meters, GeoConstants.DefaultMaxClusterDiameterMeters );

        var filter = new RemoveClusters( builder.LoggerFactory ) { MaximumClusterDiameter = maxClusterDiameter };

        builder.AddImportFilter( filter );

        return builder;
    }

    public static RouteBuilder.RouteBuilder RemoveGarminMessagePoints(
        this RouteBuilder.RouteBuilder builder
    )
    {
        builder.AddImportFilter( new RemoveGarminMessagePoints( builder.LoggerFactory ) );
        return builder;
    }

    public static RouteBuilder.RouteBuilder ExportToGpx(
        this RouteBuilder.RouteBuilder builder,
        string filePath,
        Distance2? maxGap = null
    )
    {
        if( !builder.TryCreateExporter<GpxExporter>( filePath, out var exporter, maxGap ))
            return builder;

        builder.AddExportTarget( exporter! );

        return builder;
    }

    private static bool TryCreateExporter<TExporter>(
        this RouteBuilder.RouteBuilder builder,
        string filePath,
        out IFileExporter? exporter,
        Distance2? maxGap = null
    )
        where TExporter : class, IFileExporter
    {
        exporter = null;

        if( string.IsNullOrEmpty( filePath ) )
            return false;

        exporter = Activator.CreateInstance( typeof( TExporter ),
                                             new object?[] { builder.LoggerFactory } ) as TExporter;

        if( exporter == null )
            return false;

        exporter.FilePath = filePath;

        if( maxGap != null )
            exporter.AddFilter( new SkipPoints( builder.LoggerFactory ) { MaximumGap = maxGap } );

        if( !exporter.FilePath.Equals( filePath, StringComparison.OrdinalIgnoreCase ) )
            builder.Logger?.LogWarning( "Changed file extension to {ext}", exporter.FileType );

        return true;
    }

    public static RouteBuilder.RouteBuilder ExportToKml(
        this RouteBuilder.RouteBuilder builder,
        string filePath,
        Distance2? maxGap = null
    )
    {
        if (!builder.TryCreateExporter<KmlExporter2>(filePath, out var exporter, maxGap))
            return builder;

        builder.AddExportTarget(exporter!);

        return builder;
    }

    public static RouteBuilder.RouteBuilder ExportToKmz(
        this RouteBuilder.RouteBuilder builder,
        string filePath,
        Distance2? maxGap = null
    )
    {
        if (!builder.TryCreateExporter<KmzExporter2>(filePath, out var exporter, maxGap))
            return builder;

        builder.AddExportTarget(exporter!);

        return builder;
    }

    public static RouteBuilder.RouteBuilder SendStatusReportsTo(
        this RouteBuilder.RouteBuilder builder,
        Func<StatusInformation, Task> statusReporter
    )
    {
        builder.StatusReporter = statusReporter;
        return builder;
    }

    public static RouteBuilder.RouteBuilder StatusInterval( this RouteBuilder.RouteBuilder builder, int interval )
    {
        builder.StatusInterval = interval < 0 ? GeoConstants.DefaultStatusInterval : interval;
        return builder;
    }

    public static RouteBuilder.RouteBuilder SendMessagesTo(
        this RouteBuilder.RouteBuilder builder,
        Func<ProcessingMessage, Task> mesgReporter
    )
    {
        builder.MessageReporter = mesgReporter;
        return builder;
    }
}
