# J4JSoftware.GeoProcessor Example

A simple example of using the library might look like this:

```csharp
// loggerFactory is an instance of ILoggerFactory created elsewhere
// importPath is the path to a GPX file you want to import and process
// bingKey is your Bing Maps API key, obtained elsewhere
var routeBuilder = new RouteBuilder(loggerFactory)
                        .AddGpxFile( importPath )
                        .RemoveGarminMessagePoints()
                        .MergeRoutes()
                        .RemoveClusters()
                        .ConsolidateAlongBearing()
                        .SnapWithBing( bingKey )
                        .ExportToKml( "path-to-some-kml-file.kml" );

await routeBuilder.BuildAsync();
```

This would generate snapped routes in a newly-created KML file.

A more involved example might look like this (*based on code from the `Test.GeoProcessor` project; I've removed some value checking and the `XUnit` test calls for clarity.*):

```csharp
public async Task TestBuilder( 
    string importFile, 
    SnapperType snapperType, 
    params FileType[] exportTypes )
{
    Enum.TryParse<FileType>( Path.GetExtension( importFile )[ 1.. ], true, out var importType );

    var importPath = Path.Combine( Environment.CurrentDirectory, importType.ToString(), importFile );

    var routeBuilder = Services.GetService<RouteBuilder>()
                                .RemoveGarminMessagePoints();

    var exportFiles = new string[ exportTypes.Length ];
    
    // this block creates a desktop folder for storing the exported files
    for( var idx = 0; idx < exportFiles.Length; idx++ )
    {
        exportFiles[ idx ] = Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.Desktop ),
                                            "GeoProcessor Output",
                                            $"{Path.GetFileNameWithoutExtension( importPath )}.{exportTypes[ idx ]}" );
        exportFiles[ idx ].Should().NotBeNullOrEmpty();
    }

    Directory.CreateDirectory( Path.GetDirectoryName( exportFiles[ 0 ] )! );

    foreach( var exportFile in exportFiles )
    {
        if( File.Exists( exportFile ) )
            File.Delete( exportFile );
    }

    routeBuilder = routeBuilder!.MergeRoutes()
                                .RemoveClusters()
                                .ConsolidateAlongBearing()
                                .SendProgressReportsTo( LogStatus )
                                .SendStatusReportsTo( LogMessage );

    switch( snapperType )
    {
        case SnapperType.Bing:
            routeBuilder = routeBuilder.SnapWithBing( Config.BingKey );
            break;

        case SnapperType.Google:
            routeBuilder = routeBuilder.SnapWithGoogle( Config.GoogleKey );
            break;

        default:
            throw new InvalidEnumArgumentException();
    }

    // add importer
    switch( importType )
    {
        case FileType.Gpx:
            routeBuilder = routeBuilder.AddGpxFile( importPath )
                                        .RemoveGarminMessagePoints();
            break;

        case FileType.Kml:
            routeBuilder = routeBuilder.AddKmlFile( importPath )
                                        .RemoveGarminMessagePoints();
            break;

        case FileType.Kmz:
            routeBuilder = routeBuilder.AddKmzFile(importPath)
                                        .RemoveGarminMessagePoints();
            break;

        default:
            throw new InvalidEnumArgumentException();
    }

    // add exporters for each file type we want to create
    for( var idx = 0; idx < exportTypes.Length; idx++ )
    {
        switch( exportTypes[ idx ] )
        {
            case FileType.Gpx:
                routeBuilder = routeBuilder.ExportToGpx( exportFiles[ idx ] );
                break;

            case FileType.Kml:
                routeBuilder = routeBuilder.ExportToKml( exportFiles[ idx ] );
                break;

            case FileType.Kmz:
                routeBuilder = routeBuilder.ExportToKmz( exportFiles[ idx ] );
                break;

            default:
                throw new InvalidEnumArgumentException();
        }
    }

    await routeBuilder.BuildAsync();
}
```

`SnapperType` and `FileType` are enums I created to make it easier to select a particular route snapping service and export to a particular type of file:

```csharp
public enum SnapperType
{
    Bing,
    Google
}

public enum FileType
{
    Gpx,
    Kml,
    Kmz
}
```
