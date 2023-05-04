#region license

// Copyright 2021 Mark A. Olbert
// 
// This library or program 'Test.GeoProcessor' is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public License as
// published by the Free Software Foundation, either version 3 of the License,
// or (at your option) any later version.
// 
// This library or program is distributed in the hope that it will be useful, but
// WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// General Public License for more details.
// 
// You should have received a copy of the GNU General Public License along with
// this library or program.  If not, see <https://www.gnu.org/licenses/>.

#endregion

using System;
using System.IO;
using System.Threading;
using FluentAssertions;
using J4JSoftware.DependencyInjection;
using J4JSoftware.GeoProcessor;
using Xunit;

namespace Test.GeoProcessor;

public class GeoTests : TestBase
{
    [ Theory ]
    [ InlineData( "testData.gpx" ) ]
    [ InlineData( "testData.kml" ) ]
    [ InlineData( "testDataSpaces.kml" ) ]
    [ InlineData( "testData.kmz" ) ]
    public async void TestImport( string dataFile )
    {
        var fileInfo = new InputFileInfo
        {
            FilePath = Path.Combine( Environment.CurrentDirectory, dataFile )
        };

        fileInfo.Type.Should().NotBe( ImportType.Unknown );

        var importer = GetImporter( fileInfo.Type );
        importer.Should().NotBeNull();

        var cancellationToken = new CancellationToken();

        var pointSets = await importer.ImportAsync( fileInfo.FilePath, cancellationToken );
        pointSets.Should().NotBeNull();
    }

    [ Theory ]
    [ InlineData( "testData.gpx", ExportType.KML ) ]
    [ InlineData( "testData.kml", ExportType.KML ) ]
    [ InlineData( "testDataSpaces.kml", ExportType.KML ) ]
    [ InlineData( "testData.kmz", ExportType.KML ) ]
    [ InlineData( "testData.gpx", ExportType.KMZ ) ]
    [ InlineData( "testData.kml", ExportType.KMZ ) ]
    [ InlineData( "testDataSpaces.kml", ExportType.KMZ ) ]
    [ InlineData( "testData.kmz", ExportType.KMZ ) ]
    public async void TestExport( string dataFile, ExportType type )
    {
        var fileInfo = new InputFileInfo
        {
            FilePath = Path.Combine( Environment.CurrentDirectory, dataFile )
        };

        fileInfo.Type.Should().NotBe( ImportType.Unknown );

        var importer = GetImporter( fileInfo.Type );
        importer.Should().NotBeNull();

        var cancellationToken = new CancellationToken();

        var pointSets = await importer.ImportAsync( fileInfo.FilePath, cancellationToken );
        pointSets.Should().NotBeNull();


        var outInfo = GetExportConfig();
        outInfo.Should().NotBeNull();

        outInfo.OutputFile.FilePath = fileInfo.FilePath;
        outInfo.OutputFile.Type = type;
        outInfo.OutputFile.FileNameWithoutExtension = $"{outInfo.OutputFile.FileNameWithoutExtension}-processed";

        var exporter = GetExporter( type );
        exporter.Should().NotBeNull();

        ( await exporter.ExportAsync( pointSets![ 0 ], 0, cancellationToken ) ).Should().BeTrue();
    }
}