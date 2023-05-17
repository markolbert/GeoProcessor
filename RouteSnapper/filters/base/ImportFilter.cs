#region copyright
// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// ImportFilter.cs
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
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.RouteSnapper;

public abstract class ImportFilter : IImportFilter
{
    public static ImportFilterAttributeInfo? GetAttributeInfo( Type filterType )
    {
        if( TryGetFilterAttribute<ImportFilterAttribute>( filterType, out var attr1 ) )
            return new ImportFilterAttributeInfo( attr1!.FilterName, attr1 );

        if( TryGetFilterAttribute<BeforeImportFiltersAttribute>( filterType, out var attr2 ) )
            return new ImportFilterAttributeInfo( attr2!.FilterName, attr2 );

        if( TryGetFilterAttribute<AfterImportFiltersAttribute>( filterType, out var attr3 ) )
            return new ImportFilterAttributeInfo( attr3!.FilterName, attr3 );

        return TryGetFilterAttribute<PostSnappingFilterAttribute>( filterType, out var attr4 )
            ? new ImportFilterAttributeInfo( attr4!.FilterName, attr4 )
            : null;
    }

    public static bool TryGetFilterAttribute<TAttr>( Type filterType, out TAttr? attribute )
        where TAttr : Attribute
    {
        attribute = null;

        if( TryGetFilterAttribute( filterType, typeof( TAttr ), out var temp ) )
            attribute = temp as TAttr;

        return attribute != null;
    }

    public static bool TryGetFilterAttribute( Type filterType, Type attrType, out Attribute? attribute )
    {
        attribute = filterType.GetCustomAttribute( attrType );
        return attribute != null;
    }

    protected ImportFilter(
        ILoggerFactory? loggerFactory
    )
    {
        Logger = loggerFactory?.CreateLogger( GetType() );

        var type = GetType();
        var attrInfo = GetAttributeInfo( type );

        if( string.IsNullOrEmpty( attrInfo?.Name ) )
        {
            Logger?.LogCritical( "Import filter {type} not decorated with a valid filter attribute", type );

            throw new NullReferenceException( $"Route processor {type} not decorated with a valid filter attribute" );
        }

        FilterName = attrInfo.Name;

        if( !TryGetFilterAttribute( type, attrInfo.Attribute.GetType(), out var attr ) )
            return;

        FilterDescription = attr switch
        {
            ImportFilterAttribute normal => normal.Description,
            BeforeImportFiltersAttribute before => before.Description,
            AfterImportFiltersAttribute after => after.Description,
            PostSnappingFilterAttribute post => post.Description,
            _ => null
        };

        Category = attr switch
        {
            ImportFilterAttribute normal => normal.Category,
            BeforeImportFiltersAttribute before => before.Category,
            AfterImportFiltersAttribute after => after.Category,
            PostSnappingFilterAttribute post=> post.Category,
            _ => throw new ArgumentException( $"Unsupported import filter attribute {attr!.GetType()}" )
        };

        Priority = attr switch
        {
            ImportFilterAttribute normal => normal.Priority,
            BeforeImportFiltersAttribute before => before.Priority,
            AfterImportFiltersAttribute after => after.Priority,
            PostSnappingFilterAttribute post => post.Priority,
            _ => throw new ArgumentException($"Unsupported import filter attribute {attr.GetType()}")
        };
    }

    protected ILogger? Logger { get; }

    public string FilterName { get; }
    public ImportFilterCategory Category { get; }
    public uint Priority { get; }
    public string? FilterDescription { get; }

    public abstract List<Route> Filter( List<Route> input );
}
