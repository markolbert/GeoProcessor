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
using Autofac;
using Autofac.Features.Indexed;
using FluentAssertions;
using J4JSoftware.DependencyInjection;
using J4JSoftware.GeoProcessor;
using J4JSoftware.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Test.GeoProcessor
{
    public class CompositionRoot : ConsoleRoot
    {
        private static CompositionRoot? _compRoot;

        public static CompositionRoot Default
        {
            get
            {
                if( _compRoot == null )
                {
                    _compRoot = new CompositionRoot();
                    _compRoot.Build();
                }

                return _compRoot!;
            }
        }

        private CompositionRoot()
            : base( "J4JSoftware", "Test.GeoProcessor" )
        {
        }

        protected override void ConfigureLogger( J4JLoggerConfiguration loggerConfig )
        {
            loggerConfig.AddEnricher<CallingContextEnricher>();
            loggerConfig.SerilogConfiguration.WriteTo.Debug();
        }

        protected override void SetupDependencyInjection( HostBuilderContext hbc, ContainerBuilder builder )
        {
            base.SetupDependencyInjection( hbc, builder );

            builder.RegisterModule<AutofacGeoProcessorModule>();

            builder.Register( c => hbc.Configuration.Get<GeoConfig>() )
                .AsImplementedInterfaces()
                .SingleInstance();
        }

        protected override void SetupConfigurationEnvironment( IConfigurationBuilder builder )
        {
            base.SetupConfigurationEnvironment( builder );

            builder.SetBasePath( Environment.CurrentDirectory )
                .AddJsonFile( Path.Combine( Environment.CurrentDirectory, "appConfig.json" ), false, false )
                .AddUserSecrets<CompositionRoot>();
        }

        public IImporter GetImporter( ImportType type )
        {
            var importers = Host?.Services.GetRequiredService<IIndex<ImportType, IImporter>>();
            importers.Should().NotBeNull();

            importers!.TryGetValue( type, out var retVal ).Should().BeTrue();

            return retVal;
        }

        public IExporter GetExporter( ExportType type )
        {
            var exporters = Host?.Services.GetRequiredService<IIndex<ExportType, IExporter>>();
            exporters.Should().NotBeNull();

            exporters!.TryGetValue( type, out var retVal ).Should().BeTrue();

            return retVal;
        }

        public IExportConfig GetExportConfig()
        {
            return Host?.Services.GetRequiredService<IExportConfig>()!;
        }
    }
}