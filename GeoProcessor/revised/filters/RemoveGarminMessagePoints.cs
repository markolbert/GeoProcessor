using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.GeoProcessor;

[BeforeAllImportFilter("Remove Garmin Message Points", 0)]
public class RemoveGarminMessagePoints : ImportFilter
{
    public RemoveGarminMessagePoints(
        ILoggerFactory? loggerFactory
    )
    :base( loggerFactory)
    {
    }

    public override List<IImportedRoute> Filter( List<IImportedRoute> input )
    {
        if( input.Any() )
            return input.Where( route => route.All( x => x.Description == null ) ).ToList();

        Logger?.LogInformation( "Nothing to filter" );
        return input;
    }
}
