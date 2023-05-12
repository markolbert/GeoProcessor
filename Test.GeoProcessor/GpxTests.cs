#region copyright
// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// GpxTests.cs
//
// This file is part of JumpForJoy Software's Test.GeoProcessor.
// 
// Test.GeoProcessor is free software: you can redistribute it and/or modify it 
// under the terms of the GNU General Public License as published by the 
// Free Software Foundation, either version 3 of the License, or 
// (at your option) any later version.
// 
// Test.GeoProcessor is distributed in the hope that it will be useful, but 
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
// or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License 
// for more details.
// 
// You should have received a copy of the GNU General Public License along 
// with Test.GeoProcessor. If not, see <https://www.gnu.org/licenses/>.
#endregion

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
        if (File.Exists(kmlExport))
            File.Delete(kmlExport);

        var kmzExport = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "TestKml.kmz");
        if (File.Exists(kmzExport))
            File.Delete(kmzExport);

        routeBuilder!.SnapWithBing( Config.BingKey )
                     .MergeRoutes()
                     .RemoveClusters()
                     .ConsolidateAlongBearing()
                     .RemoveGarminMessagePoints()
                     .AddGpxFile( importPath )
                     .ExportToGpx( gpxExport, new Distance( UnitType.Meters, 500 ) )
                     .ExportToKml( kmlExport, new Distance( UnitType.Meters, 500 ) )
                     .ExportToKmz( kmzExport, new Distance( UnitType.Meters, 500 ) )
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
