﻿#region copyright
// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// Coordinate2.cs
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

namespace J4JSoftware.GeoProcessor;

public class Coordinate2
{
    public Coordinate2(
        double latitude,
        double longitude,
        InterpolationState interpolationState = InterpolationState.NotInterpolated
    )
    {
        Latitude = latitude;
        Longitude = longitude;
        InterpolationState = interpolationState;
    }

    public double Latitude { get; }
    public double Longitude { get; }
    public InterpolationState InterpolationState { get; }

    public double? Elevation { get; set; }
    public DateTime? Timestamp { get; set; }
    public string? Description { get; set; }
}