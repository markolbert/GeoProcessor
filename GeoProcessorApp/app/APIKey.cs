#region license

// Copyright 2021 Mark A. Olbert
// 
// This library or program 'GeoProcessorApp' is free software: you can redistribute it
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

using System;
using System.Text.Json.Serialization;
using J4JSoftware.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace J4JSoftware.GeoProcessor
{
    public class APIKey
    {
        public ProcessorType Type { get; set; }
        public string EncryptedValue { get; set; } = string.Empty;

        [ JsonIgnore ]
        public string Value { get; set; } = string.Empty;
    }
}