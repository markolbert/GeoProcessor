using System.Collections.Generic;
using System.Drawing;
using System.Xml.Linq;

namespace J4JSoftware.KMLProcessor
{
    public class KmlDocument
    {
        public KmlDocument(AppConfig config)
        {
            RouteName = config.DefaultRouteName;
            RouteWidth = config.RouteWidth;
            RouteColor = config.RouteColor;
            RouteHighlightColor = config.RouteHighlightColor;
        }

        protected XElement? CoordinatesElement { get; private set; }

        //public bool IsValid => FilePath != null && XDocument != null && CoordinatesElement != null && Points.Count > 0;
        //public string? FilePath { get; private set; }
        //public XDocument? XDocument { get; private set; }
        public LinkedList<Coordinate> Points { get; set; } = new LinkedList<Coordinate>();
        public int Count => Points.Count;
        public string RouteName { get; set; }
        public int RouteWidth { get; set; }
        public Color RouteColor { get; set; }
        public Color RouteHighlightColor { get; set; }

        //public async Task<bool> LoadAsync( string filePath, CancellationToken cancellationToken )
        //{
            //if( !File.Exists( filePath ) )
            //{
            //    _logger.Error<string>( "File '{0}' does not exist", filePath );
            //    return false;
            //}

            //FilePath = filePath;

            //try
            //{
            //    using var readStream = File.OpenText( FilePath );
            //    XDocument = await XDocument.LoadAsync( readStream, LoadOptions.None, cancellationToken );
            //}
            //catch( Exception e )
            //{
            //    _logger.Error<string, string>( "Could not load file '{0}', exception was '{1}'", FilePath, e.Message );
            //    return false;
            //}

            //if( cancellationToken.IsCancellationRequested )
            //{
            //    _logger.Information( "File load cancelled" );
            //    return false;
            //}

            //CoordinatesElement = XDocument.Descendants()
            //    .SingleOrDefault( x => x.Name.LocalName == "coordinates" );

            //if( CoordinatesElement == null )
            //{
            //    _logger.Error( "Could not find 'coordinates' element in XDocument" );
            //    return false;
            //}

            //var coordRaw = CoordinatesElement.Value.Replace( "\t", "" )
            //    .Replace( "\n", "" );

            //if( cancellationToken.IsCancellationRequested )
            //{
            //    _logger.Information( "File load cancelled" );
            //    return false;
            //}

            //Points = new LinkedList<Coordinate>();
            //LinkedListNode<Coordinate>? prevPoint = null;

            //foreach( var coordText in coordRaw.Split( ' ', StringSplitOptions.RemoveEmptyEntries ) )
            //{
            //    prevPoint = Points.Count == 0
            //        ? Points.AddFirst( new Coordinate( coordText ) )
            //        : Points.AddAfter( prevPoint!, new Coordinate( coordText ) );

            //    if( !cancellationToken.IsCancellationRequested )
            //        continue;

            //    _logger.Information( "File load cancelled" );
            //    return false;
            //}

            //return true;
        //}

        //public async Task<bool> SaveAsync( string outputFile, CancellationToken cancellationToken )
        //{
        //    if( !IsValid )
        //    {
        //        _logger.Error( "Cannot save invalid KmlDocument" );
        //        return false;
        //    }

        //    var sb = new StringBuilder();
        //    sb.AppendLine();

        //    foreach( var point in Points )
        //        // having NO SPACES between these three arguments is INCREDIBLY IMPORTANT.
        //        // the Google Earth importer parses based on spaces (but ignores tabs, linefeeds & newlines)
        //        // also note that LONGITUDE is emitted FIRST!!!
        //        sb.AppendLine( $"\t\t\t{point.Longitude},{point.Latitude},0 " );

        //    CoordinatesElement!.Value = sb.ToString();

        //    await using var writeStream = File.CreateText( outputFile );

        //    await XDocument!.SaveAsync( writeStream, SaveOptions.None, cancellationToken );

        //    await writeStream.FlushAsync();
        //    writeStream.Close();

        //    return true;
        //}

        //public int CoalescePointsByDistance( Distance maxDist, double origDistanceMultiplier = 3.0 )
        //{
        //    var pointSet = new List<Coordinate>();

        //    var curStartingPoint = Points.First;
        //    var origCount = Count;

        //    while( curStartingPoint?.Next != null )
        //    {
        //        pointSet.Clear();
        //        pointSet.Add( curStartingPoint.Value );

        //        var curEndingPoint = curStartingPoint!.Next;

        //        while( curEndingPoint != null )
        //        {
        //            var mostRecentDistance = KMLExtensions
        //                .GetDistance( curEndingPoint.Previous!.Value, curEndingPoint.Value );

        //            var distanceFromOrigin = KMLExtensions
        //                .GetDistance( curStartingPoint.Value, curEndingPoint.Value );

        //            if( mostRecentDistance > maxDist || distanceFromOrigin > origDistanceMultiplier * maxDist )
        //            {
        //                Points.Remove( curStartingPoint );

        //                break;
        //            }

        //            var nextEnd = curEndingPoint.Next;

        //            Points.Remove( curEndingPoint );

        //            pointSet.Add( curEndingPoint.Value );

        //            curEndingPoint = nextEnd;
        //        }

        //        if( pointSet.Count > 0 )
        //        {
        //            var avg = new Coordinate(
        //                pointSet.Average( x => x.Latitude ),
        //                pointSet.Average( x => x.Longitude )
        //            );

        //            if( curEndingPoint == null )
        //                Points.AddLast( avg );
        //            else Points.AddBefore( curEndingPoint!, avg );
        //        }

        //        curStartingPoint = curEndingPoint;
        //    }

        //    return origCount - Count;
        //}

        //public int CoalescePointsByBearing( double maxBearingDelta )
        //{
        //    var curStartingPoint = Points.First;
        //    var origCount = Count;

        //    var sameBearing = new List<LinkedListNode<Coordinate>>();

        //    while( curStartingPoint?.Next != null )
        //    {
        //        var curEndingPoint = curStartingPoint.Next;

        //        sameBearing.Clear();

        //        while( curEndingPoint != null )
        //        {
        //            var (avgBearing, sdBearing) = curStartingPoint.GetBearingStatistics( curEndingPoint! );

        //            var mostRecentBearing = KMLExtensions.GetBearing(
        //                curEndingPoint!.Previous!.Value,
        //                curEndingPoint.Value );

        //            if( Math.Abs( avgBearing - mostRecentBearing ) > maxBearingDelta )
        //            {
        //                curStartingPoint = curEndingPoint;

        //                if( sameBearing.Count > 1 )
        //                    foreach( var node in sameBearing )
        //                        Points.Remove( node );

        //                break;
        //            }

        //            sameBearing.Add( curEndingPoint );

        //            curEndingPoint = curEndingPoint.Next;
        //        }
        //    }

        //    return origCount - Count;
        //}

        //public async Task<int> CoalescePoints(CancellationToken cancellationToken)
        //{
        //    var routePts = await _distProc.ProcessAsync(Points, cancellationToken);

        //    if (routePts == null)
        //    {
        //        _logger.Error("Coalescing nearby points failed");
        //        return 0;
        //    }

        //    Points = routePts;

        //    return Points.Count;
        //}

        //public async Task<int> SnapToRoute( CancellationToken cancellationToken )
        //{
        //    var routePts = await _routeProc.ProcessAsync( Points, cancellationToken );

        //    if( routePts == null )
        //    {
        //        _logger.Error( "Snapping points to a route failed" );
        //        return 0;
        //    }

        //    Points = routePts;

        //    return Points.Count;
        //}
    }
}