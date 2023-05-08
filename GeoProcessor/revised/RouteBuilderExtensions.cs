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

    public static RouteBuilder.RouteBuilder SnapWithBing( this RouteBuilder.RouteBuilder builder )
    {
        builder.SnapProcessor = new BingProcessor2( builder.LoggerFactory );
        return builder;
    }

    public static RouteBuilder.RouteBuilder SnapWithGoogle(this RouteBuilder.RouteBuilder builder)
    {
        builder.SnapProcessor = new GoogleProcessor2(builder.LoggerFactory);
        return builder;
    }

    public static RouteBuilder.RouteBuilder ConsolidatePoints(
        this RouteBuilder.RouteBuilder builder,
        double minPointGap,
        double maxOverallGap
    )
    {
        var filter = new ConsolidatePoints( builder.LoggerFactory )
        {
            MaximumOverallGap = maxOverallGap, MinimumPointGap = minPointGap
        };

        builder.AddImportFilter( filter );

        return builder;
    }

    public static RouteBuilder.RouteBuilder InterpolatePoints(
        this RouteBuilder.RouteBuilder builder,
        double maxSeparation = GeoConstants.DefaultMaxPointSeparationKm
    )
    {
        var filter = new InterpolatePoints( builder.LoggerFactory ) { MaximumPointSeparation = maxSeparation };

        builder.AddImportFilter( filter );

        return builder;
    }

    public static RouteBuilder.RouteBuilder MergeRoutes(
        this RouteBuilder.RouteBuilder builder,
        double maxRouteGap = GeoConstants.DefaultMaxRouteGapMeters
    )
    {
        var filter = new MergeRoutes( builder.LoggerFactory ) { MaximumRouteGap = maxRouteGap };

        builder.AddImportFilter( filter );

        return builder;
    }

    public static RouteBuilder.RouteBuilder RemoveClusters(
        this RouteBuilder.RouteBuilder builder,
        double maxClusterDiameter = GeoConstants.DefaultMaxClusterDiameterMeters
    )
    {
        var filter = new RemoveClusters( builder.LoggerFactory ) { MaximumClusterDiameter = maxClusterDiameter };

        builder.AddImportFilter( filter );

        return builder;
    }

    public static RouteBuilder.RouteBuilder RouteColor(
        this RouteBuilder.RouteBuilder builder,
        Color color
    )
    {
        builder.RouteColor = color;
        return builder;
    }

    public static RouteBuilder.RouteBuilder RouteHighlightColor(
        this RouteBuilder.RouteBuilder builder,
        Color color
    )
    {
        builder.RouteHighlightColor = color;
        return builder;
    }

    public static RouteBuilder.RouteBuilder RouteWidth(
        this RouteBuilder.RouteBuilder builder,
        int width
    )
    {
        builder.RouteWidth = width < GeoConstants.MinimumRouteWidth ? GeoConstants.MinimumRouteWidth : width;
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
