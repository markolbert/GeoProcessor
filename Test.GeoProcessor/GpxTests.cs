using System;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using J4JSoftware.GeoProcessor;
using Microsoft.Extensions.DependencyInjection;
using J4JSoftware.GeoProcessor.RouteBuilder;
using Xunit;

namespace Test.GeoProcessor;

public class GpxTests : TestBase
{
    [ Theory ]
    [ InlineData( "Sherman Pass.gpx" ) ]
    public async Task CreateBuilder( string fileName )
    {
        var importPath = Path.Combine( Environment.CurrentDirectory, "gpx", fileName );
        File.Exists( importPath ).Should().BeTrue();

        var routeBuilder = Services.GetService<RouteBuilder>();
        routeBuilder.Should().NotBeNull();

        var gpxExport = Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.Desktop ), "TestGpx.gpx" );

        if( File.Exists( gpxExport ) )
            File.Delete( gpxExport );

        var kmlExport = Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.Desktop ), "TestKml.kml" );

        if( File.Exists( kmlExport ) )
            File.Delete( kmlExport );

        routeBuilder!.SnapWithBing( Config.BingKey )
                     .MergeRoutes()
                     .RemoveClusters()
                     .ConsolidateAlongBearing()
                     .RemoveGarminMessagePoints()
                     .AddGpxFile( importPath )
                     .ExportToGpx( gpxExport, new Distance2( UnitType.Meters, 500 ) )
                     .ExportToKml( kmlExport, new Distance2( UnitType.Meters, 500 ) )
                     .SendStatusReportsTo( LogStatus )
                     .SendMessagesTo( LogMessage );

        var result = await routeBuilder!.BuildAsync();

        result.Should().NotBeNull();
        result!.Count.Should().BeGreaterThan( 0 );

        routeBuilder.SnapProcessor.Should().NotBeNull();
        routeBuilder.SnapProcessor!.ProblemMessages.Should().BeEmpty();
    }

    [ Theory ]
    [InlineData(37.4596, -122.28607, 37.44124, -122.2508,3,3.723)]
    public void TestHaversine(double lat1, double long1, double lat2, double long2, int rounding, double result)
    {
        var distance = GeoExtensions.GetDistance( lat1, long1,lat2, long2 );
        Math.Round( distance, rounding ).Should().Be( result );
    }
}
