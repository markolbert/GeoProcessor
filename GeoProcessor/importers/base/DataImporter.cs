#region copyright
// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// DataImporter.cs
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
using J4JSoftware.GeoProcessor.RouteBuilder;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.GeoProcessor;

public class DataImporter : Importer
{
    public DataImporter(
        ILoggerFactory? loggerFactory = null
    )
        : base( null, loggerFactory )
    {
    }

#pragma warning disable CS1998
    protected override async Task<List<ImportedRoute>> ImportInternalAsync(
        DataToImportBase toImport,
        CancellationToken ctx
    )
#pragma warning restore CS1998
    {
        var retVal = new List<ImportedRoute>();

        if( toImport is not DataToImport dataToImport )
        {
            Logger?.LogError( "Expected a {correct} but got a {incorrect} instead",
                              typeof( DataToImport ),
                              toImport.GetType() );

            return retVal;
        }

        var route = new ImportedRoute( new Points( dataToImport.Points ) )
        {
            RouteName = dataToImport.Name
        };

        retVal.Add( route );

        return retVal;
    }
}
