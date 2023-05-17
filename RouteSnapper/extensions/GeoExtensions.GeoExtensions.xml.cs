#region copyright
// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// GeoExtensions.GeoExtensions.xml.cs
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
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace J4JSoftware.RouteSnapper;

public static partial class GeoExtensions
{
    public static bool IsNamedElement( this XElement element, string elementName ) =>
        element.Name.LocalName.Equals( elementName, StringComparison.OrdinalIgnoreCase );

    public static string? GetFirstDescendantValue( this XElement element, string elementName ) =>
        element.Descendants()
               .FirstOrDefault( x => x.Name.LocalName.Equals( elementName, StringComparison.OrdinalIgnoreCase ) )
              ?.Value;

    public static List<XElement> GetNamedDescendants( this XElement element, string descendantName ) =>
        element.Descendants()
               .Where( x => x.Name.LocalName.Equals( descendantName, StringComparison.OrdinalIgnoreCase ) )
               .ToList();

    public static IEnumerable<XElement> GetNamedDescendants( this List<XElement> element, string descendantName ) =>
        element.SelectMany( x => x.Descendants()
                                  .Where( y => y.Name.LocalName.Equals( descendantName,
                                                                        StringComparison.OrdinalIgnoreCase ) ) );

    public static bool TryParseAttribute<T>( this XElement element, string attributeName, out T value )
        where T : struct
    {
        value = default;

        var text = element.Attributes()
                          .FirstOrDefault(
                               x => x.Name.LocalName.Equals( attributeName, StringComparison.OrdinalIgnoreCase ) )
                         ?.Value;

        if( string.IsNullOrEmpty( text ) )
            return false;

        try
        {
            value = (T) System.Convert.ChangeType( text, typeof( T ) );
        }
        catch
        {
            return false;
        }

        return true;
    }

    public static bool TryParseFirstDescendantValue<T>( this XElement parent, string descendentName, out T value )
        where T : struct
    {
        value = default;

        var text = parent.GetFirstDescendantValue( descendentName );

        if( string.IsNullOrEmpty( text ) )
            return false;

        try
        {
            value = (T) System.Convert.ChangeType( text, typeof( T ) );
        }
        catch
        {
            return false;
        }

        return true;
    }
}
