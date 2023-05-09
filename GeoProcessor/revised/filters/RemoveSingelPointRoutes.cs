using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.GeoProcessor;

[BeforeAllImportFilter("Remove Single Point Routes", 10)]
public class RemoveSinglePointRoutes : ImportFilter
{
    public RemoveSinglePointRoutes(
        ILoggerFactory? loggerFactory
    )
    :base( loggerFactory)
    {
    }

    public override List<IImportedRoute> Filter( List<IImportedRoute> input )
    {
        if( input.Any() )
            return input.Where( x => x.NumPoints > 1 ).ToList();

        Logger?.LogInformation( "Nothing to filter" );
        return input;
    }
}
