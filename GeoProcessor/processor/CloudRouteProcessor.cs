#region copyright
// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// CloudRouteProcessor.cs
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
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.GeoProcessor;

public class CloudRouteProcessor : RouteProcessor
{
    protected CloudRouteProcessor(
        IImportConfig config,
        ProcessorType processorType,
        ILoggerFactory? loggerFactory )
        : base( config, processorType, loggerFactory )
    {
        ApiKey = config.APIKey;
    }

    protected string ApiKey { get; }

    public override async Task<LinkedList<Coordinate>?> ProcessAsync(
        LinkedList<Coordinate> nodes,
        CancellationToken cancellationToken )
    {
        if( string.IsNullOrEmpty( ApiKey ) )
        {
            Logger?.LogError( "{type}: APIKey is undefined", GetType() );
            return null;
        }

        var retVal = new LinkedList<Coordinate>();

        var chunks = ChunkPoints( InterpolatePoints( nodes ) );

        var ptsSinceLastReport = 0;

        foreach( var coordinates in chunks )
        {
            if( cancellationToken.IsCancellationRequested )
                return null;

            if( !await ProcessChunkAsync( coordinates, retVal, cancellationToken ) )
                return null;

            ptsSinceLastReport += coordinates.Count;

            if( ptsSinceLastReport < ReportingInterval )
                continue;

            OnReportingInterval( ptsSinceLastReport );
            ptsSinceLastReport -= ReportingInterval;
        }

        return retVal;
    }

    protected virtual async Task<bool> ProcessChunkAsync(
        List<Coordinate> coordinates,
        LinkedList<Coordinate> outputNodes,
        CancellationToken cancellationToken )
    {
        var snappedPts = await ExecuteRequestAsync( coordinates, cancellationToken );

        if( snappedPts == null )
            return false;

        UpdateOutputList( snappedPts, outputNodes );

        return true;
    }

    private List<Coordinate> InterpolatePoints( LinkedList<Coordinate> nodes )
    {
        var retVal = new List<Coordinate>();

        Coordinate? prevPt = null;

        foreach( var curPt in nodes )
        {
            if( prevPt == null )
            {
                retVal.Add( curPt );
                prevPt = curPt;

                continue;
            }

            var distance = GeoExtensions.GetDistance( prevPt, curPt );

            if( distance <= Configuration.MaxSeparation )
            {
                retVal.Add( curPt );
                prevPt = curPt;

                continue;
            }

            // interpolate
            var numPtsNeeded = Convert.ToInt32(
                Math.Ceiling( distance.GetValue( Configuration.MaxSeparation.Unit ) /
                              Configuration.MaxSeparation.OriginalValue ) );

            var deltaLat = ( curPt.Latitude - prevPt.Latitude ) / numPtsNeeded;
            var deltaLong = ( curPt.Longitude - prevPt.Longitude ) / numPtsNeeded;

            for( var idx = 0; idx < numPtsNeeded; idx++ )
                retVal.Add( new Coordinate
                {
                    Latitude = prevPt.Latitude + ( idx + 1 ) * deltaLat,
                    Longitude = prevPt.Longitude + ( idx + 1 ) * deltaLong
                } );

            prevPt = curPt;
        }

        return retVal;
    }

    private List<List<Coordinate>> ChunkPoints( List<Coordinate> points )
    {
        var retVal = new List<List<Coordinate>>();

        var ptsChunked = 0;

        while( ptsChunked < points.Count - 1 )
        {
            var coordinates = points.Skip( ptsChunked )
                                    .Take( ProcessorType.MaxPointsPerRequest() )
                                    .ToList();

            retVal.Add( coordinates );

            ptsChunked += coordinates.Count;
        }

        return retVal;
    }
}