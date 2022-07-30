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
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Autofac;
using Autofac.Features.Indexed;
using FluentAssertions;
using J4JSoftware.Configuration.CommandLine;
using J4JSoftware.DependencyInjection;
using J4JSoftware.GeoProcessor;
using J4JSoftware.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Test.GeoProcessor
{
    public class TestBase
    {
        private readonly IHost _host;

        protected TestBase()
        {
            _host = CreateHost();
        }

        protected IImporter GetImporter(ImportType type)
        {
            var importers = _host?.Services.GetRequiredService<IIndex<ImportType, IImporter>>();
            importers.Should().NotBeNull();

            importers!.TryGetValue(type, out var retVal).Should().BeTrue();

            return retVal;
        }

        protected IExporter GetExporter(ExportType type)
        {
            var exporters = _host?.Services.GetRequiredService<IIndex<ExportType, IExporter>>();
            exporters.Should().NotBeNull();

            exporters!.TryGetValue(type, out var retVal).Should().BeTrue();

            return retVal;
        }

        protected IExportConfig GetExportConfig()
        {
            return _host?.Services.GetRequiredService<IExportConfig>()!;
        }

        private IHost CreateHost()
        {
            var hostConfig = new J4JHostConfiguration(AppEnvironment.Console)
                .Publisher("J4JSoftware")
                .ApplicationName("Tests.GeoProcessor")
                .AddApplicationConfigurationFile("appConfig.json")
                .LoggerInitializer(LoggerInitializer)
                .FilePathTrimmer(FilePathTrimmer)
                .AddConfigurationInitializers(SetupConfigurationEnvironment)
                .AddDependencyInjectionInitializers(SetupDependencyInjection);

            hostConfig.MissingRequirements.Should().Be( J4JHostRequirements.AllMet );

            var retVal = hostConfig!.Build();
            retVal.Should().NotBeNull();

            return retVal!;
        }

        private void LoggerInitializer( IConfiguration config, J4JHostConfiguration hostConfig, J4JLoggerConfiguration loggerConfig ) =>
            loggerConfig.SerilogConfiguration.WriteTo.Debug();

        private void SetupConfigurationEnvironment( IConfigurationBuilder builder )
        {
            builder.AddUserSecrets<TestBase>();
        }

        private void SetupDependencyInjection( HostBuilderContext hbc, ContainerBuilder builder )
        {
            builder.RegisterModule<AutofacGeoProcessorModule>();

            builder.Register( c => hbc.Configuration.Get<GeoConfig>() )
                .AsImplementedInterfaces()
                .SingleInstance();
        }

        private string FilePathTrimmer(
            Type? loggedType,
            string callerName,
            int lineNum,
            string srcFilePath)
        {
            return CallingContextEnricher.DefaultFilePathTrimmer(loggedType,
                callerName,
                lineNum,
                CallingContextEnricher.RemoveProjectPath(srcFilePath, GetProjectPath()));
        }

        private string GetProjectPath([CallerFilePath] string filePath = "")
        {
            var dirInfo = new DirectoryInfo(Path.GetDirectoryName(filePath)!);

            while (dirInfo.Parent != null)
            {
                if (dirInfo.EnumerateFiles("*.csproj").Any())
                    break;

                dirInfo = dirInfo.Parent;
            }

            return dirInfo.FullName;
        }
    }
}