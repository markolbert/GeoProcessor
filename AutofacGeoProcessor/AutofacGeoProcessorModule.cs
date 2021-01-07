using System.Linq;
using System.Reflection;
using Autofac;
using Module = Autofac.Module;

namespace J4JSoftware.GeoProcessor
{
    public class AutofacGeoProcessorModule : Module
    {
        protected override void Load( ContainerBuilder builder )
        {
            base.Load( builder );

            builder.RegisterType<DataProtection>()
                .As<IDataProtection>()
                .SingleInstance();

            // register route processors
            builder.RegisterAssemblyTypes(typeof(DataProtection).Assembly)
                .Where(t => typeof(IRouteProcessor).IsAssignableFrom(t)
                            && !t.IsAbstract
                            && t.GetConstructors().Any()
                            && GeoExtensions.GetTargetType<RouteProcessorAttribute>(t) != null)
                .Keyed<IRouteProcessor>(t => t.GetCustomAttribute<RouteProcessorAttribute>()!.Type)
                .AsImplementedInterfaces()
                .SingleInstance();

            // register exporters
            builder.RegisterAssemblyTypes( typeof(DataProtection).Assembly )
                .Where( t => typeof(IExporter).IsAssignableFrom( t )
                             && !t.IsAbstract
                             && t.GetConstructors().Any()
                             && GeoExtensions.GetTargetType<ExporterAttribute>( t ) != null )
                .Keyed<IExporter>( t => t.GetCustomAttribute<ExporterAttribute>()!.Type )
                .AsImplementedInterfaces()
                .SingleInstance();

            // register importers
            builder.RegisterAssemblyTypes(typeof(DataProtection).Assembly)
                .Where(t => typeof(IImporter).IsAssignableFrom(t)
                            && !t.IsAbstract
                            && t.GetConstructors().Any()
                            && GeoExtensions.GetTargetType<ImporterAttribute>(t) != null)
                .Keyed<IImporter>(t => t.GetCustomAttribute<ImporterAttribute>()!.Type)
                .AsImplementedInterfaces()
                .SingleInstance();
        }
    }
}
