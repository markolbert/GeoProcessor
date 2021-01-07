using System;
using System.IO;
using System.Threading;
using Autofac.Extensions.DependencyInjection;
using FluentAssertions;
using J4JSoftware.GeoProcessor;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace Test.GeoProcessor
{
    public class GeoTests
    {
        [Theory]
        [InlineData("testData.gpx")]
        [InlineData("testData.kml")]
        [InlineData("testDataSpaces.kml")]
        [InlineData("testData.kmz")]
        public async void TestImport( string dataFile )
        {
            var fileInfo = new InputFileInfo
            {
                FilePath = Path.Combine( Environment.CurrentDirectory, dataFile )
            };

            fileInfo.Type.Should().NotBe( ImportType.Unknown );

            var importer = CompositionRoot.Default.GetImporter( fileInfo.Type );
            importer.Should().NotBeNull();

            var cancellationToken = new CancellationToken();

            var pointSets = await importer.ImportAsync( fileInfo.FilePath, cancellationToken );
            pointSets.Should().NotBeNull();
        }

        [Theory]
        [InlineData("testData.gpx", ExportType.KML )]
        [InlineData("testData.kml", ExportType.KML)]
        [InlineData("testDataSpaces.kml", ExportType.KML)]
        [InlineData("testData.kmz", ExportType.KML)]
        [InlineData("testData.gpx", ExportType.KMZ)]
        [InlineData("testData.kml", ExportType.KMZ)]
        [InlineData("testDataSpaces.kml", ExportType.KMZ)]
        [InlineData("testData.kmz", ExportType.KMZ)]
        public async void TestExport(string dataFile, ExportType type )
        {
            var fileInfo = new InputFileInfo
            {
                FilePath = Path.Combine(Environment.CurrentDirectory, dataFile)
            };

            fileInfo.Type.Should().NotBe(ImportType.Unknown);

            var importer = CompositionRoot.Default.GetImporter(fileInfo.Type);
            importer.Should().NotBeNull();

            var cancellationToken = new CancellationToken();

            var pointSets = await importer.ImportAsync(fileInfo.FilePath, cancellationToken);
            pointSets.Should().NotBeNull();


            var outInfo = CompositionRoot.Default.GetExportConfig();
            outInfo.Should().NotBeNull();

            outInfo.OutputFile.FilePath = fileInfo.FilePath;
            outInfo.OutputFile.Type = type;
            outInfo.OutputFile.FileNameWithoutExtension = $"{outInfo.OutputFile.FileNameWithoutExtension}-processed";

            var exporter = CompositionRoot.Default.GetExporter( type );
            exporter.Should().NotBeNull();

            ( await exporter.ExportAsync( pointSets![ 0 ], 0, cancellationToken ) ).Should().BeTrue();
        }
    }
}
