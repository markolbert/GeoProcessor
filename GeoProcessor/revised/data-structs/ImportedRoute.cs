using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using BingMapsRESTToolkit;
using GoogleApi.Entities.Maps.Directions.Response;

namespace J4JSoftware.GeoProcessor;

public class ImportedRoute : IImportedRoute
{
    public ImportedRoute()
    {
        Points = new List<Coordinate2>();
    }

    public ImportedRoute(
        List<Coordinate2> points
    )
    {
        Points = points;
    }

    public ImportedRoute Copy() => new( new List<Coordinate2>( Points ) ) { RouteName = RouteName };

    public string RouteName { get; set; } = "Unnamed Route";
    public string Description { get; set; } = string.Empty;

    public int NumPoints => Points.Count;
    public List<Coordinate2> Points { get; }

    public IEnumerator<Coordinate2> GetEnumerator() => ( (IEnumerable<Coordinate2>) Points ).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}