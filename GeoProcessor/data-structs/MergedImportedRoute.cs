#region copyright
// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// MergedImportedRoute.cs
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

using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace J4JSoftware.GeoProcessor;

public class MergedImportedRoute : IImportedRoute
{
    public MergedImportedRoute(
        IImportedRoute routeA, 
        IImportedRoute routeB, 
        RouteConnectionType connectionType
    )
    {
        RouteA = routeA;
        RouteB = routeB;
        ConnectionType = connectionType;
    }

    public string RouteName => $"{RouteA.RouteName}->{RouteB.RouteName}";

    public string Description
    {
        get
        {
            var aDesc = string.IsNullOrEmpty( RouteA.Description ) ? " " : RouteA.Description;
            var bDesc = string.IsNullOrEmpty( RouteB.Description ) ? " " : RouteB.Description;

            return $"{aDesc}->{bDesc}";
        }
    }

    int IImportedRoute.NumPoints(Distance? minSeparation, Distance? maxOverallGap)
    {
        throw new System.NotImplementedException();
    }

    public IImportedRoute RouteA { get; }
    public IImportedRoute RouteB { get;}
    public RouteConnectionType ConnectionType { get; }

    public int NumPoints(Distance? minSeparation = null, Distance? maxOverallGap = null) =>
        GetFilteredPoints(minSeparation, maxOverallGap).Count();

    public bool Any(Distance? minSeparation = null, Distance? maxOverallGap = null) =>
        GetFilteredPoints(minSeparation, maxOverallGap).Any();

    public IEnumerable<Point> GetFilteredPoints(Distance? minSeparation = null, Distance? maxOverallGap = null)
    {
        IEnumerable<Point>? toAdd;

        switch (ConnectionType)
        {
            case RouteConnectionType.StartToStart:
            case RouteConnectionType.EndToEnd:
                var reversed = RouteB.GetFilteredPoints(minSeparation, maxOverallGap).ToList();
                reversed.Reverse();
                toAdd = reversed;

                break;

            case RouteConnectionType.StartToEnd:
            case RouteConnectionType.EndToStart:
                toAdd = RouteB.GetFilteredPoints(minSeparation, maxOverallGap);
                break;

            default:
                // shouldn't ever get here
                throw new InvalidEnumArgumentException(
                    $"Unsupported {typeof(RouteConnectionType)} value '{ConnectionType}'");
        }

        switch (ConnectionType)
        {
            case RouteConnectionType.StartToStart:
            case RouteConnectionType.StartToEnd:
                foreach (var point in toAdd)
                {
                    yield return point;
                }

                foreach (var point in RouteA.GetFilteredPoints(minSeparation, maxOverallGap))
                {
                    yield return point;
                }

                break;

            case RouteConnectionType.EndToStart:
            case RouteConnectionType.EndToEnd:
                foreach (var point in RouteA.GetFilteredPoints(minSeparation, maxOverallGap))
                {
                    yield return point;
                }

                foreach (var point in toAdd)
                {
                    yield return point;
                }

                break;
        }
    }
}
