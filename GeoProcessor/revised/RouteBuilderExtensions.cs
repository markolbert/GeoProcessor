using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;

namespace J4JSoftware.GeoProcessor;

public static class RouteBuilderExtensions
{
    public static RouteBuilder.RouteBuilder AddSourceFile(
        this RouteBuilder.RouteBuilder builder,
        string filePath,
        string fileType,
        bool lineStringsOnly = true,
        bool throwOnFailure = false
    )
    {
        if ( builder.AddSourceFile(fileType, filePath, lineStringsOnly)
         || !throwOnFailure )
            return builder;

        throw new ArgumentException( $"Invalid source file or file type" );
    }

    public static RouteBuilder.RouteBuilder UseProcessor(
        this RouteBuilder.RouteBuilder builder,
        string processor,
        string apiKey,
        TimeSpan? requestTimeout = null,
        bool throwOnFailure = false
    )
    {
        requestTimeout ??= GeoConstants.DefaultRequestTimeout;

        if( builder.UseProcessor( processor, apiKey, requestTimeout.Value ) || !throwOnFailure )
            return builder;

        throw new ArgumentException( $"Unknown processor" );
    }

    public static RouteBuilder.RouteBuilder AddImportFilter( this RouteBuilder.RouteBuilder builder, string filterName )
    {
        builder.AddImportFilter( filterName );
        return builder;
    }

    public static RouteBuilder.RouteBuilder MergeImportedData( this RouteBuilder.RouteBuilder builder )
    {
        builder.MergeImportedData = true;
        return builder;
    }

    public static RouteBuilder.RouteBuilder DoNotMergeImportedData(this RouteBuilder.RouteBuilder builder)
    {
        builder.MergeImportedData = false;
        return builder;
    }

    public static RouteBuilder.RouteBuilder AddCoordinates(
        this RouteBuilder.RouteBuilder builder,
        string name,
        List<Coordinate2> coordinates,
        double minPointGap = 0,
        double minOverallGap = 0
    )
    {
        minPointGap = minPointGap < 0 ? 0 : minPointGap;
        minOverallGap = minOverallGap < 0 ? 0 : minOverallGap;

        builder.AddCoordinates( name, coordinates, minPointGap, minOverallGap );
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
