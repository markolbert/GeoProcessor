#region copyright
// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// GeoConstants.general.cs
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

using System;

namespace J4JSoftware.RouteSnapper;

public partial class GeoConstants
{
    public const double RadiansPerDegree = Math.PI / 180;
    public const double DegreesPerRadian = 180 / Math.PI;
    public const double EarthRadiusInMiles = 3958.8;
    public const double EarthRadiusInKilometers = 6371;
    public const double FeetPerMeter = 3.28084;
}
