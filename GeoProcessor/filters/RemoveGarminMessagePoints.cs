﻿#region copyright
// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// RemoveGarminMessagePoints.cs
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
using System.Linq;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.GeoProcessor;

[BeforeImportFilters(DefaultFilterName, 0)]
public class RemoveGarminMessagePoints : ImportFilter
{
    public const string DefaultFilterName = "Remove Garmin Message Points";

    public RemoveGarminMessagePoints(
        ILoggerFactory? loggerFactory
    )
    :base( loggerFactory)
    {
    }

    public override List<Route> Filter( List<Route> input )
    {
        if( input.Any() )
            return input.Where( route => route.Points.All( x => x.Description == null ) ).ToList();

        Logger?.LogInformation( "Nothing to filter" );
        return input;
    }
}
