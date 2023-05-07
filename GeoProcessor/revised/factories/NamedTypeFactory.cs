using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.GeoProcessor;

public abstract class NamedTypeFactory<TCreate>
    where TCreate : class
{
    private record TypeInfo( Type Type, Attribute Attribute );

    private readonly List<Assembly> _assemblies = new();
    private readonly Dictionary<string, TypeInfo> _itemTypes = new( StringComparer.OrdinalIgnoreCase );
    private readonly ILogger? _logger;

    protected NamedTypeFactory(
        ILoggerFactory? loggerFactory = null
    )
    {
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
            var attrInfo = GetAttributeInfo( itemType );
            if( attrInfo == null  )
                continue;

            if( !_itemTypes.TryAdd( attrInfo.Name, new TypeInfo( itemType, attrInfo.Attribute ) ) )
                _logger?.LogError( "Attempting to add duplicate {type} '{name}' to factory, ignoring",
                                   itemType,
                                   attrInfo.Name );
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

    protected abstract NamedTypeAttributeInfo? GetAttributeInfo( Type itemType );

    public TCreate? this[ string itemName ]
    {
        get
        {
            if( _itemTypes.TryGetValue( itemName, out var typeInfo ) )
                return CreateInstance( typeInfo.Type );

            _logger?.LogWarning( "Unsupported {type} '{name}'", typeof( TCreate ), itemName );
            return null;
        }
    }

    public Attribute? GetAttribute( string itemName ) =>
        _itemTypes.TryGetValue( itemName, out var typeInfo ) ? typeInfo.Attribute : null;

    protected abstract TCreate? CreateInstance( Type type );
}