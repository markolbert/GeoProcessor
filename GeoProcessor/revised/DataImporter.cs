using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using J4JSoftware.GeoProcessor.RouteBuilder;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.GeoProcessor;

public class DataImporter : Importer
{
    public DataImporter(
        ILoggerFactory? loggerFactory = null
    )
        : base( null, loggerFactory )
    {
    }

#pragma warning disable CS1998
    protected override async Task<List<ImportedRoute>> ImportInternalAsync(
        DataToImportBase toImport,
        CancellationToken ctx
    )
#pragma warning restore CS1998
    {
        var retVal = new List<ImportedRoute>();

        if( toImport is not DataToImport dataToImport )
        {
            Logger?.LogError( "Expected a {correct} but got a {incorrect} instead",
                              typeof( DataToImport ),
                              toImport.GetType() );

            return retVal;
        }

        var folder = new ImportedRoute( dataToImport.Name, new List<Coordinate2>() );

        Coordinate2? prevPt = null;
        Coordinate2? curStartPt = null;

        foreach( var curPt in dataToImport.Coordinates )
        {
            var curPair = prevPt == null ? null : new PointPair( prevPt, curPt );
            var distanceFromPrevPt = curPair?.GetDistance() ?? double.MaxValue;

            var startPair = curStartPt == null ? null : new PointPair( curStartPt, curPt );
            var distanceFromStartPt = startPair?.GetDistance() ?? double.MaxValue;

            prevPt = curPt;

            if( distanceFromPrevPt <= dataToImport.MinPointGap
            && distanceFromStartPt <= dataToImport.MinOverallGap )
                continue;

            folder.Coordinates.Add( curPt );
        }

        retVal.Add( folder );
        return retVal;
    }
}
