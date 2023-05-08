using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace J4JSoftware.GeoProcessor;

public class MergedImportedRoute : IImportedRoute
{
    public MergedImportedRoute(
        IImportedRoute routeA, 
        IImportedRoute routeB, 
        RouteConnectionType connectionType
    )
    {
        RouteA = routeA;
        RouteB = routeB;
        ConnectionType = connectionType;
    }

    public string RouteName => $"{RouteA.RouteName}->{RouteB.RouteName}";

    public string Description
    {
        get
        {
            var aDesc = string.IsNullOrEmpty( RouteA.Description ) ? " " : RouteA.Description;
            var bDesc = string.IsNullOrEmpty( RouteB.Description ) ? " " : RouteB.Description;

            return $"{aDesc}->{bDesc}";
        }
    }

    public IImportedRoute RouteA { get; }
    public IImportedRoute RouteB { get;}
    public RouteConnectionType ConnectionType { get; }

    public int NumPoints => this.Count();

    public IEnumerator<Coordinate2> GetEnumerator()
    {
        IEnumerable<Coordinate2>? toAdd;

        switch (ConnectionType)
        {
            case RouteConnectionType.StartToStart:
            case RouteConnectionType.EndToEnd:
                var reversed = RouteB.ToList();
                reversed.Reverse();
                toAdd = reversed;

                break;

            case RouteConnectionType.StartToEnd:
            case RouteConnectionType.EndToStart:
                toAdd = RouteB;
                break;

            default:
                // shouldn't ever get here
                throw new InvalidEnumArgumentException(
                    $"Unsupported {typeof(RouteConnectionType)} value '{ConnectionType}'");
        }

        switch (ConnectionType)
        {
            case RouteConnectionType.StartToStart:
            case RouteConnectionType.StartToEnd:
                foreach( var point in toAdd )
                {
                    yield return point;
                }

                foreach (var point in RouteA)
                {
                    yield return point;
                }

                break;

            case RouteConnectionType.EndToStart:
            case RouteConnectionType.EndToEnd:
                foreach (var point in RouteA)
                {
                    yield return point;
                }

                foreach (var point in toAdd)
                {
                    yield return point;
                }

                break;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
