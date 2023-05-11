using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.GeoProcessor;

[BeforeImportFilters(DefaultFilterName, 20)]
public class RemoveSinglePointRoutes : ImportFilter
{
    public const string DefaultFilterName = "Remove Single Point Routes";

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
