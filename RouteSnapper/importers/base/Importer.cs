#region copyright
// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// Importer.cs
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
using System.Threading;
using System.Threading.Tasks;
using J4JSoftware.RouteSnapper.RouteBuilder;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.RouteSnapper;

public abstract class Importer : MessageBasedTask, IImporter
{
    protected Importer(
        string? mesgPrefix = null,
        ILoggerFactory? loggerFactory = null
    )
    : base(mesgPrefix, loggerFactory)
    {
    }

    public async Task<List<Route>?> ImportAsync( DataToImportBase toImport, CancellationToken ctx = default )
    {
        await OnProcessingStarted();

        var retVal = await ImportInternalAsync( toImport, ctx );

        await OnProcessingEnded();

        return retVal;
    }

    protected abstract Task<List<Route>?> ImportInternalAsync( DataToImportBase toImport, CancellationToken ctx );
}
