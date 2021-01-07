using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Autofac.Features.Indexed;
using FluentAssertions;
using J4JSoftware.DependencyInjection;
using J4JSoftware.GeoProcessor;
using J4JSoftware.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Test.GeoProcessor
{
    public class CompositionRoot : J4JCompositionRoot
    {
        static CompositionRoot()
        {
            Default = new CompositionRoot
            {
                UseConsoleLifetime = true,
            };

            Default.Initialize();
        }

        public static CompositionRoot Default { get; }

        private CompositionRoot()
        {
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

            builder.AddUserSecrets<CompositionRoot>();
        }

        public IImporter GetImporter( ImportType type )
        {
            var importers = Host?.Services.GetRequiredService<IIndex<ImportType, IImporter>>();
            importers.Should().NotBeNull();

            importers!.TryGetValue( type, out var retVal ).Should().BeTrue();

            return retVal;
        }

        public IExporter GetExporter(ExportType type)
        {
            var exporters = Host?.Services.GetRequiredService<IIndex<ExportType, IExporter>>();
            exporters.Should().NotBeNull();

            exporters!.TryGetValue(type, out var retVal).Should().BeTrue();

            return retVal;
        }

        public IExportConfig GetExportConfig () => Host?.Services.GetRequiredService<IExportConfig>();
    }
}
