#region copyright
// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// ProcessRouteResult.cs
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

namespace J4JSoftware.GeoProcessor;

public class ProcessRouteResult
{
    public static ProcessRouteResult Failed { get; } = new ProcessRouteResult();

    private readonly List<ExportedRoute> _results = new();

    public ProcessRouteStatus Status
    {
        get
        {
            if( !_results.Any() )
                return ProcessRouteStatus.NoResults;

            if( _results.All( x => x.Status == SnapProcessStatus.IsValid ) )
                return ProcessRouteStatus.AllSucceeded;

            return _results.All( x => x.Status != SnapProcessStatus.IsValid )
                ? ProcessRouteStatus.AllFailed
                : ProcessRouteStatus.SomeSucceeded;
        }
    }

    public ReadOnlyCollection<ExportedRoute> Results => _results.AsReadOnly();

    public void AddResult( ExportedRoute result ) => _results.Add( result );
}
