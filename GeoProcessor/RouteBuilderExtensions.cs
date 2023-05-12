﻿#region copyright
// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// RouteBuilderExtensions.cs
//
// This file is part of JumpForJoy Software's GeoProcessor.
// 
// GeoProcessor is free software: you can redistribute it and/or modify it 
// under the terms of the GNU General Public License as published by the 
// Free Software Foundation, either version 3 of the License, or 
// (at your option) any later version.
// 
// GeoProcessor is distributed in the hope that it will be useful, but 
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
// or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License 
// for more details.
// 
// You should have received a copy of the GNU General Public License along 
// with GeoProcessor. If not, see <https://www.gnu.org/licenses/>.
#endregion

using J4JSoftware.GeoProcessor.RouteBuilder;
using System;
using System.Collections.Generic;
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
        var importer = new GpxImporter( builder.LoggerFactory ) { LineStringsOnly = lineStringsOnly };

        if( !File.Exists( filePath ) )
            builder.Logger?.LogError( "{filePath} does not exist", filePath );
        else builder.AddDataSource( new FileToImport( filePath, importer ) );

        return builder;
    }

    public static RouteBuilder.RouteBuilder AddCoordinates(
        this RouteBuilder.RouteBuilder builder,
        string collectionName,
        IEnumerable<Coordinates> coordinates
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
        builder.SnapProcessor = new BingProcessor( maxPtsPerRequest, builder.LoggerFactory ) { ApiKey = apiKey };
        return builder;
    }

    public static RouteBuilder.RouteBuilder SnapWithGoogle(
        this RouteBuilder.RouteBuilder builder,
        string apiKey,
        int maxPtsPerRequest = 100
    )
    {
        builder.SnapProcessor =
            new GoogleProcessor( maxPtsPerRequest, builder.LoggerFactory ) { ApiKey = apiKey };

        return builder;
    }

    public static RouteBuilder.RouteBuilder ConsolidatePoints(
        this RouteBuilder.RouteBuilder builder,
        Distance? minPointGap = null,
        Distance? maxOverallGap = null
    )
    {
        minPointGap ??= new Distance( UnitType.Meters, GeoConstants.DefaultMinimumPointGapMeters );
        maxOverallGap ??= new Distance( UnitType.Meters, GeoConstants.DefaultMaximumOverallGapMeters );

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
        Distance? maxConsolDist = null
    )
    {
        // 2.5 km is the max distance between points for Bing
        maxConsolDist ??= new Distance( UnitType.Kilometers, 2.5 );

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
        Distance? maxSeparation = null
    )
    {
        maxSeparation ??= new Distance( UnitType.Kilometers, GeoConstants.DefaultMaxPointSeparationKm );

        var filter = new InterpolatePoints( builder.LoggerFactory ) { MaximumPointSeparation = maxSeparation };

        builder.AddImportFilter( filter );

        return builder;
    }

    public static RouteBuilder.RouteBuilder MergeRoutes(
        this RouteBuilder.RouteBuilder builder,
        Distance? maxRouteGap = null
    )
    {
        maxRouteGap ??= new Distance( UnitType.Meters, GeoConstants.DefaultMaxRouteGapMeters );

        var filter = new MergeRoutes( builder.LoggerFactory ) { MaximumRouteGap = maxRouteGap };

        builder.AddImportFilter( filter );

        return builder;
    }

    public static RouteBuilder.RouteBuilder RemoveClusters(
        this RouteBuilder.RouteBuilder builder,
        Distance? maxClusterDiameter = null
    )
    {
        maxClusterDiameter ??= new Distance( UnitType.Meters, GeoConstants.DefaultMaxClusterDiameterMeters );

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
        Distance? maxGap = null
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
        Distance? maxGap = null
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
        Distance? maxGap = null
    )
    {
        if (!builder.TryCreateExporter<KmlExporter>(filePath, out var exporter, maxGap))
            return builder;

        builder.AddExportTarget(exporter!);

        return builder;
    }

    public static RouteBuilder.RouteBuilder ExportToKmz(
        this RouteBuilder.RouteBuilder builder,
        string filePath,
        Distance? maxGap = null
    )
    {
        if (!builder.TryCreateExporter<KmzExporter>(filePath, out var exporter, maxGap))
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
