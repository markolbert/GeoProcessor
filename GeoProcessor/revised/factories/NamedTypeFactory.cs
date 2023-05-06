using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.GeoProcessor;

public abstract class NamedTypeFactory<TCreate, TAttr>
    where TCreate : class
    where TAttr : Attribute
{
    private readonly Func<TAttr, string> _itemName;
    private readonly List<Assembly> _assemblies = new();
    private readonly Dictionary<string, Type> _itemTypes = new( StringComparer.OrdinalIgnoreCase );
    private readonly ILogger? _logger;

    protected NamedTypeFactory(
        Expression<Func<TAttr, string>> attrNameBinder,
        ILoggerFactory? loggerFactory = null
    )
    {
        _itemName = attrNameBinder.Compile();
        _logger = loggerFactory?.CreateLogger( GetType() );
    }

    public void ScanAssemblies( params Assembly[] assemblies ) => _assemblies.AddRange( assemblies );
    public void ScanAssemblies( params Type[] types ) => _assemblies.AddRange( types.Select( t => t.Assembly ) );

    public bool InitializeFactory( bool scanDefault = true )
    {
        if( scanDefault )
            _assemblies.Add( GetType().Assembly );

        if( !_assemblies.Any() )
            return false;

        _itemTypes.Clear();

        foreach( var itemType in _assemblies.Distinct()
                                            .SelectMany( a => a.GetTypes() )
                                            .Where( IncludeType ) )
        {
            var attr = itemType.GetCustomAttribute<TAttr>();
            if( attr == null )
                continue;

            var itemName = _itemName( attr );

            if( !_itemTypes.TryAdd( itemName, itemType ) )
                _logger?.LogError( "Attempting to add duplicate {type} '{name}' to factory, ignoring",
                                   itemType,
                                   itemName );
        }

        return true;
    }

    protected virtual bool IncludeType( Type type )
    {
        if( !type.IsAssignableTo( typeof( TCreate ) )
        || type.IsAbstract )
            return false;

        var retVal = false;

        foreach( var ctor in type.GetConstructors() )
        {
            retVal |= CheckConstructor( ctor );
        }

        return retVal;
    }

    protected abstract bool CheckConstructor( ConstructorInfo ctor );

    public TCreate? this[ string itemName ]
    {
        get
        {
            if( _itemTypes.TryGetValue( itemName, out var type ) )
                return CreateInstance( type );

            _logger?.LogWarning( "Unsupported {type} '{name}'", typeof( TCreate ), itemName );
            return null;
        }
    }

    protected abstract TCreate? CreateInstance( Type type );
}
