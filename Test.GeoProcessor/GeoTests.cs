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
    }
}
