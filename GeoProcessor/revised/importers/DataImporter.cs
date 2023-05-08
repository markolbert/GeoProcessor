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

        var route = new ImportedRoute( new List<Coordinate2>( dataToImport.Coordinates ) )
        {
            RouteName = dataToImport.Name
        };

        retVal.Add( route );

        return retVal;
    }
}
