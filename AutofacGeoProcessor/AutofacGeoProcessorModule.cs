#region license

// Copyright 2021 Mark A. Olbert
// 
// This library or program 'AutofacGeoProcessor' is free software: you can redistribute it
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

            // register route processors
            builder.RegisterAssemblyTypes( typeof(IRouteProcessor).Assembly )
                .Where( t => typeof(IRouteProcessor).IsAssignableFrom( t )
                             && !t.IsAbstract
                             && t.GetConstructors().Any()
                             && GeoExtensions.GetTargetType<RouteProcessorAttribute>( t ) != null )
                .Keyed<IRouteProcessor>( t => t.GetCustomAttribute<RouteProcessorAttribute>()!.Type )
                .AsImplementedInterfaces()
                .SingleInstance();

            // register exporters
            builder.RegisterAssemblyTypes( typeof(IRouteProcessor).Assembly )
                .Where( t => typeof(IExporter).IsAssignableFrom( t )
                             && !t.IsAbstract
                             && t.GetConstructors().Any()
                             && GeoExtensions.GetTargetType<ExporterAttribute>( t ) != null )
                .Keyed<IExporter>( t => t.GetCustomAttribute<ExporterAttribute>()!.Type )
                .AsImplementedInterfaces()
                .SingleInstance();

            // register importers
            builder.RegisterAssemblyTypes( typeof(IRouteProcessor).Assembly )
                .Where( t => typeof(IImporter).IsAssignableFrom( t )
                             && !t.IsAbstract
                             && t.GetConstructors().Any()
                             && GeoExtensions.GetTargetType<ImporterAttribute>( t ) != null )
                .Keyed<IImporter>( t => t.GetCustomAttribute<ImporterAttribute>()!.Type )
                .AsImplementedInterfaces()
                .SingleInstance();
        }
    }
}