using System;
using System.Collections.Generic;
using System.Linq;

namespace J4JSoftware.RouteSnapper;

public class Route
{
    private readonly List<Point> _pointsToAdd = new();

    public string? RouteName { get; set; }
    public string? Description { get; set; }

    public List<Point> Points { get; } = new();

    public IEnumerable<List<Point>> GetRouteChunks( RouteChunkInfo chunkInfo )
    {
        var chunk = new List<Point>();

        Point? prevPt = null;

        foreach( var point in Points )
        {
            _pointsToAdd.Clear();
            
            if( prevPt == null )
                _pointsToAdd.Add( point );
            else
            {
                var ptPair = new PointPair( prevPt, point );
                var curSep = ptPair.GetDistance();

                if ( curSep <= chunkInfo.MaximumSeparation )
                    _pointsToAdd.Add(point);
                else AccumulateInterpolatedPoints( ptPair, curSep, chunkInfo.MaximumSeparation );
            }

            prevPt = point;

            foreach( var ptToAdd in _pointsToAdd )
            {
                chunk.Add( ptToAdd );

                if( chunk.Count < chunkInfo.ChunkSize )
                    continue;

                yield return chunk;
                chunk.Clear();
            }
        }

        if( chunk.Any())
            yield return chunk;
    }

    private void AccumulateInterpolatedPoints( PointPair ptPair, Distance curSep, Distance maxSep )
    {
        var steps = (int) Math.Ceiling( ( curSep / maxSep ).Value );

        var deltaLat = ( ptPair.Second.Latitude - ptPair.First.Latitude ) / steps;
        var deltaLong = ( ptPair.Second.Longitude - ptPair.First.Longitude ) / steps;

        var deltaElevation = ptPair.First.Elevation.HasValue && ptPair.Second.Elevation.HasValue
            ? ( ptPair.Second.Elevation - ptPair.First.Elevation ) / steps
            : null;

        var deltaTime = ptPair.First.Timestamp.HasValue && ptPair.Second.Timestamp.HasValue
            ? ( ptPair.Second.Timestamp - ptPair.First.Timestamp ) / steps
            : null;

        for( var idx = 0; idx <= steps; idx++ )
        {
            var interpolated = new Point
            {
                Latitude = ptPair.First.Latitude + idx * deltaLat,
                Longitude = ptPair.First.Longitude + idx * deltaLong,
                Interpolated = true
            };

            if( deltaElevation.HasValue )
                interpolated.Elevation = ptPair.First.Elevation + idx * deltaElevation;

            if( deltaTime.HasValue )
                interpolated.Timestamp = ptPair.First.Timestamp + idx * deltaTime;

            _pointsToAdd.Add( interpolated );
        }
    }
}