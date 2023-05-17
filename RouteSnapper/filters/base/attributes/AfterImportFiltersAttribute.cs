#region copyright
// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// AfterImportFiltersAttribute.cs
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

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
internal class AfterImportFiltersAttribute : Attribute
{
    public AfterImportFiltersAttribute(
        string filterName,
        uint priority,
        string? description = null
    )
    {
        FilterName = filterName;
        Priority = priority;
        Description = description;
    }

    public string FilterName { get; }
    public uint Priority { get; }
    public ImportFilterCategory Category => ImportFilterCategory.AfterUserDefined;
    public string? Description { get; }
}
