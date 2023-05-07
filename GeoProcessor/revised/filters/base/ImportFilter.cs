﻿using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.GeoProcessor;

public abstract class ImportFilter : IImportFilter
{
    public static NamedTypeAttributeInfo? GetAttributeInfo(Type filterType)
    {
        if( TryGetFilterAttribute<ImportFilterAttribute>( filterType, out var attr1 ) )
            return new NamedTypeAttributeInfo( attr1!.FilterName, attr1 );

        if( TryGetFilterAttribute<BeforeAllImportFilterAttribute>( filterType, out var attr2 ) )
            return new NamedTypeAttributeInfo( attr2!.FilterName, attr2 );

        return TryGetFilterAttribute<AfterAllImportFilterAttribute>( filterType, out var attr3 )
            ? new NamedTypeAttributeInfo( attr3!.FilterName, attr3 )
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

        if( TryGetFilterAttribute( type, attrInfo.Attribute.GetType(), out var attr ) )
            FilterDescription = attr switch
            {
                ImportFilterAttribute normal => normal.Description,
                BeforeAllImportFilterAttribute before => before.Description,
                AfterAllImportFilterAttribute after => after.Description,
                _ => null
            };
    }

    protected ILogger? Logger { get; }

    public string FilterName { get; }
    public string? FilterDescription { get; }

    public abstract List<ImportedRoute> Filter( List<ImportedRoute> input );
}
