#region copyright
// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// Exporter.cs
//
// This file is part of JumpForJoy Software's GeoProcessor.
// 
// GeoProcessor is free software: you can redistribute it and/or modify it 
// under the terms of the GNU General Public License as published by the 
// Free Software Foundation, either version 3 of the License, or 
// (at your option) any later version.
// 
// GeoProcessor is distributed in the hope that it will be useful, but 
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
// or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License 
// for more details.
// 
// You should have received a copy of the GNU General Public License along 
// with GeoProcessor. If not, see <https://www.gnu.org/licenses/>.
#endregion

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.GeoProcessor;

public abstract class Exporter : MessageBasedTask, IExporter
{
    private readonly List<IImportFilter> _importFilters = new();

    protected Exporter(
        ILoggerFactory? loggerFactory
    )
        : base( null, loggerFactory )
    {
    }

    public ReadOnlyCollection<IImportFilter> ImportFilters => _importFilters.AsReadOnly();

    public void ClearImportFilters() => _importFilters.Clear();

    public bool AddFilter( IImportFilter filter )
    {
        if( filter.Category != ImportFilterCategory.PostSnapping )
        {
            Logger?.LogError( "Filter '{name}' is not a {snapping} filter",
                              filter.FilterName,
                              typeof( ImportFilterCategory ) );
            return false;
        }

        _importFilters.Add( filter );
        return true;
    }

    public bool AddFilters( IEnumerable<IImportFilter> filters ) =>
        filters.Aggregate( true, ( current, filter ) => current & AddFilter( filter ) );

    public async Task<bool> ExportAsync( List<ImportedRoute> routes, CancellationToken ctx = default )
    {
        var filters = AdjustImportFilters();
        var toProcess = routes.Cast<IImportedRoute>().ToList();

        foreach( var filter in filters.Where( x => x.Category == ImportFilterCategory.PostSnapping )
                                      .OrderBy( x => x.Category )
                                      .ThenBy( x => x.Priority ) )
        {
            Logger?.LogInformation( "Executing {filter} filter...", filter.FilterName );
            toProcess = filter.Filter( toProcess );
        }

        Logger?.LogInformation("Filtering complete");

        return await ExportInternalAsync( toProcess, ctx );
    }

    protected virtual List<IImportFilter> AdjustImportFilters()=> _importFilters;

    protected abstract Task<bool> ExportInternalAsync( List<IImportedRoute> routes, CancellationToken ctx );
}