#region copyright
// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// Tests.cs
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
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using J4JSoftware.GeoProcessor;
using Microsoft.Extensions.DependencyInjection;
using J4JSoftware.GeoProcessor.RouteBuilder;
using Xunit;

namespace Test.GeoProcessor;

public class Tests : TestBase
{
    [ Theory ]
    [ InlineData( "Sherman Pass.gpx", SnapperType.Bing, FileType.Gpx, FileType.Kml, FileType.Kmz ) ]
    [InlineData("Sherman Pass.kml", SnapperType.Bing, FileType.Gpx, FileType.Kml, FileType.Kmz)]
    public async Task TestBuilder( string importFile, SnapperType snapperType, params FileType[] exportTypes  )
    {
        exportTypes.Length.Should().BeGreaterOrEqualTo( 1 );

        Enum.TryParse<FileType>( Path.GetExtension( importFile )[ 1.. ], true, out var importType )
            .Should()
            .BeTrue();

        var importPath = Path.Combine( Environment.CurrentDirectory, importType.ToString() , importFile );
        File.Exists( importPath )
            .Should()
            .BeTrue();

        var routeBuilder = Services.GetService<RouteBuilder>();
        routeBuilder.Should().NotBeNull();

        var exportFiles = new string[ exportTypes.Length ];
        for( var idx = 0; idx < exportFiles.Length; idx++ )
        {
            exportFiles[ idx ] = Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.Desktop ),
                                               "GeoProcessor Output",
                                               $"{Path.GetFileNameWithoutExtension( importPath )}.{exportTypes[ idx ]}" );
            exportFiles[ idx ].Should().NotBeNullOrEmpty();
        }

        Directory.CreateDirectory( Path.GetDirectoryName(exportFiles[0] )! );

        foreach( var exportFile in exportFiles )
        {
            if( File.Exists( exportFile ) )
                File.Delete( exportFile );
        }

        routeBuilder = routeBuilder!.MergeRoutes()
                                    .RemoveClusters()
                                    .ConsolidateAlongBearing()
                                    .SendStatusReportsTo( LogStatus )
                                    .SendMessagesTo( LogMessage );

        switch( snapperType )
        {
            case SnapperType.Bing:
                routeBuilder = routeBuilder.SnapWithBing( Config.BingKey );
                break;

            case SnapperType.Google:
                routeBuilder = routeBuilder.SnapWithGoogle( Config.GoogleKey );
                break;

            default:
                throw new InvalidEnumArgumentException();
        }

        switch( importType )
        {
            case FileType.Gpx:
                routeBuilder = routeBuilder.AddGpxFile(importPath)
                                           .RemoveGarminMessagePoints();
                break;

            case FileType.Kml:
                routeBuilder = routeBuilder.AddKmlFile(importPath)
                                           .RemoveGarminMessagePoints();
                break;

            default:
                throw new InvalidEnumArgumentException();
        }

        for( var idx = 0; idx < exportTypes.Length; idx++ )
        {
            switch( exportTypes[ idx ] )
            {
                case FileType.Gpx:
                    routeBuilder = routeBuilder.ExportToGpx( exportFiles[ idx ] );
                    break;

                case FileType.Kml:
                    routeBuilder = routeBuilder.ExportToKml( exportFiles[ idx ] );
                    break;

                case FileType.Kmz:
                    routeBuilder = routeBuilder.ExportToKmz( exportFiles[ idx ] );
                    break;

                default:
                    throw new InvalidEnumArgumentException();
            }
        }

        var result = await routeBuilder.BuildAsync();

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
