#region copyright
// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// RouteProcessor.cs
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.RouteSnapper;

public abstract class RouteProcessor : MessageBasedTask, IRouteProcessor
{
    protected RouteProcessor(
        int maxPtsPerRequest,
        string? mesgPrefix = null,
        ILoggerFactory? loggerFactory = null
    )
        : base( mesgPrefix, loggerFactory )
    {
        MaxPointsInRequest = maxPtsPerRequest;

        var type = GetType();
        var procAttr = type.GetCustomAttribute<RouteProcessorAttribute>();

        if( string.IsNullOrEmpty( procAttr?.Processor ) )
        {
            Logger?.LogCritical( "Route processor {type} not decorated with a valid {attr}",
                                 type,
                                 typeof( RouteProcessorAttribute ) );

            throw new NullReferenceException(
                $"Route processor {type} not decorated with a valid {typeof( RouteProcessorAttribute )}" );
        }

        ProcessorName = procAttr.Processor;
    }

    public string ProcessorName { get; }
    public List<IImportFilter> ImportFilters { get; private set; } = new();
    public string ApiKey { get; set; } = string.Empty;
    public TimeSpan RequestTimeout { get; set; } = GeoConstants.DefaultRequestTimeout;

    public int MaxPointsInRequest { get; }
    public Distance MaximumOverallPointGap { get; set; } = Distance.Zero;

    public async Task<RouteProcessorResult> ProcessRoute(List<Route> toProcess, CancellationToken ctx = default)
    {
        await OnProcessingStarted();

        foreach (var filter in ImportFilters.Where(x => x.Category != ImportFilterCategory.PostSnapping)
                                            .OrderBy(x => x.Category)
                                            .ThenBy(x => x.Priority))
        {
            Logger?.LogInformation("Executing {filter} filter...", filter.FilterName);
            toProcess = filter.Filter(toProcess);
        }

        Logger?.LogInformation("Filtering complete");

        var chunkInfo = new RouteChunkInfo(MaxPointsInRequest, MaximumOverallPointGap);

        var retVal = new RouteProcessorResult() { FilteredRoutes = toProcess };
        var snappedRoutes = new List<SnappedRoute>();

        foreach( var route in toProcess )
        {
            var snappedRoute = new SnappedRoute() { RouteName = route.RouteName, Description = route.Description };

            foreach( var chunkPoints in route.GetRouteChunks( chunkInfo ) )
            {
                var snappedPoints = await ProcessRouteChunkAsync( chunkPoints, ctx );

                if( snappedPoints != null && snappedPoints.Any() )
                    snappedRoute.SnappedPoints.AddRange( snappedPoints );
            }

            if( snappedRoute.SnappedPoints.Any() )
                snappedRoutes.Add( snappedRoute );
        }

        await OnProcessingEnded();

        if( snappedRoutes.Any() )
            retVal.SnappedRoutes = snappedRoutes;

        return retVal;
    }

    protected abstract Task<List<Point>?> ProcessRouteChunkAsync( List<Point> routeChunk, CancellationToken ctx );

    protected async Task HandleTimeoutExceptionAsync()
    {
        await SendMessage(ExpandedPhase,
                          $"Bing processing timed out after {RequestTimeout}",
                          true,
                          true,
                          LogLevel.Error);
    }

    protected async Task HandleOtherRequestExceptionAsync(string mesg)
    {
        await SendMessage(ExpandedPhase,
                          $"Bing processing failed, reply was {mesg}",
                          true,
                          true,
                          LogLevel.Error);
    }

    protected async Task HandleInvalidStatusCodeAsync(string description)
    {
        await SendMessage(ExpandedPhase,
                          $"Snap to road request failed, message was '{description}'",
                          true,
                          true,
                          LogLevel.Error);
    }
}
