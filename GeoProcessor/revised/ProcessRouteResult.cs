using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection.Metadata.Ecma335;

namespace J4JSoftware.GeoProcessor;

public class ProcessRouteResult
{
    public static ProcessRouteResult Failed { get; } = new ProcessRouteResult();

    private readonly List<ExportedRoute> _results = new();

    public ProcessRouteStatus Status
    {
        get
        {
            if( !_results.Any() )
                return ProcessRouteStatus.NoResults;

            if( _results.All( x => x.Status == SnapProcessStatus.IsValid ) )
                return ProcessRouteStatus.AllSucceeded;

            return _results.All( x => x.Status != SnapProcessStatus.IsValid )
                ? ProcessRouteStatus.AllFailed
                : ProcessRouteStatus.SomeSucceeded;
        }
    }

    public ReadOnlyCollection<ExportedRoute> Results => _results.AsReadOnly();

    public void AddResult( ExportedRoute result ) => _results.Add( result );
}
