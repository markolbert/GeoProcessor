using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using J4JSoftware.Logging;
using Microsoft.Extensions.Hosting;

namespace J4JSoftware.KMLProcessor
{
    public class App : IHostedService
    {
        private readonly IHost _host;
        private readonly IAppConfig _config;
        private readonly IHostApplicationLifetime _lifetime;
        private readonly IJ4JLogger _logger;

        public App( 
            IHost host,
            IAppConfig config,
            IHostApplicationLifetime lifetime,
            IJ4JLogger logger
        )
        {
            _host = host;
            _config = config;
            _lifetime = lifetime;

            _logger = logger;
            _logger.SetLoggedType( this.GetType() );
        }

        public async Task StartAsync( CancellationToken cancellationToken )
        {
            if( !_config.IsValid( out var error ) )
            {
                abort( error! );
                return;
            }

            using var readStream = File.OpenText( _config.InputFile );
            var xDoc = await XDocument.LoadAsync( readStream, LoadOptions.None, cancellationToken );

            var rawCoordElement = xDoc.Descendants()
                .SingleOrDefault( x => x.Name.LocalName == "coordinates" );

            var coordRaw = rawCoordElement!.Value.Replace( "\t", "" )
                .Replace( "\n", "" );

            var points = new LinkedList<Coordinate>();
            LinkedListNode<Coordinate>? prevPoint = null;

            foreach( var coordText in coordRaw.Split( ' ', StringSplitOptions.RemoveEmptyEntries ) )
            {
                prevPoint = points.Count == 0
                    ? points.AddFirst( new Coordinate( coordText ) )
                    : points.AddAfter( prevPoint!, new Coordinate( coordText ) );
            }

            var numPoints = points.Count;

            CoalescePointsByDistance( points );

            _logger.Information( "Coalesced {0:n0} points based on distance", numPoints - points.Count );
            numPoints = points.Count;

            CoalescePointsByBearing( points );
            _logger.Information("Coalesced {0:n0} points based on bearing", numPoints - points.Count);

            _logger.Information( "{0:n0} points remain in the track", points.Count );

            var sb = new StringBuilder();
            sb.AppendLine();

            foreach( var node in points )
            {
                sb.AppendLine( $"\t\t\t{node.Latitude}, {node.Longitude}" );
            }

            rawCoordElement.Value = sb.ToString();

            await using var writeStream = File.CreateText(_config.OutputFile);

            await xDoc.SaveAsync( writeStream, SaveOptions.None, cancellationToken );
            await writeStream.FlushAsync();
            writeStream.Close();

            _logger.Information<string>( "Wrote file '{0}'", _config.OutputFile );

            _lifetime.StopApplication();

            void abort( string mesg )
            {
                _logger.Fatal(mesg);
                _lifetime.StopApplication();
            }
        }

        private void CoalescePointsByDistance( LinkedList<Coordinate> points )
        {
            var pointSet = new List<Coordinate>();

            var curStartingPoint = points.First;

            while( curStartingPoint?.Next != null )
            {
                pointSet.Clear();
                pointSet.Add( curStartingPoint.Value );

                var curEndingPoint = curStartingPoint!.Next;

                while( curEndingPoint != null )
                {
                    var mostRecentDistance = CoordinateExtensions
                        .GetDistance( curEndingPoint.Previous!.Value, curEndingPoint.Value )
                        .GetValue( _config.CoalesceUnit );

                    var distanceFromOrigin = CoordinateExtensions
                        .GetDistance( curStartingPoint.Value, curEndingPoint.Value )
                        .GetValue( _config.CoalesceUnit );

                    if( mostRecentDistance > _config.CoalesceValue || distanceFromOrigin > 3 * _config.CoalesceValue )
                    {
                        points.Remove( curStartingPoint );

                        break;
                    }

                    var nextEnd = curEndingPoint.Next;

                    points.Remove( curEndingPoint );

                    pointSet.Add( curEndingPoint.Value );

                    curEndingPoint = nextEnd;
                }

                if( pointSet.Count > 0)
                {
                    var avg = new Coordinate(
                        pointSet.Average( x => x.Latitude ),
                        pointSet.Average( x => x.Longitude )
                    );

                    if( curEndingPoint == null ) 
                        points.AddLast( avg );
                    else points.AddBefore( curEndingPoint!, avg );
                }

                curStartingPoint = curEndingPoint;
            }
        }

        private void CoalescePointsByBearing(LinkedList<Coordinate> points)
        {
            var curStartingPoint = points.First;

            var sameBearing = new List<LinkedListNode<Coordinate>>();

            while (curStartingPoint?.Next != null)
            {
                var curEndingPoint = curStartingPoint.Next;

                sameBearing.Clear();

                while (curEndingPoint != null)
                {
                    var (avgBearing, sdBearing) = curStartingPoint.GetBearingStatistics( curEndingPoint! );

                    var mostRecentBearing = CoordinateExtensions.GetBearing(
                        curEndingPoint!.Previous!.Value,
                        curEndingPoint.Value );

                    if( Math.Abs( avgBearing - mostRecentBearing ) > _config.MaxBearingStdDev )
                    {
                        curStartingPoint = curEndingPoint;

                        if( sameBearing.Count > 1 )
                        {
                            foreach( var node in sameBearing )
                            {
                                points.Remove( node );
                            }
                        }

                        break;
                    }

                    sameBearing.Add( curEndingPoint );

                    curEndingPoint = curEndingPoint.Next;
                }
            }
        }

        public async Task StopAsync( CancellationToken cancellationToken )
        {
        }
    }
}