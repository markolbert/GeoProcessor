# Adding Data Sources

There are several methods available for importing files into the route snapping process, and one for adding collections of coordinates.

|Extension Method|Arguments|Comments|
|----------------|---------|--------|
|`AddGpxFile`|`string` filePath|adds a GPX file as a data source|
|`AddKmlFile`|`string` filePath|adds a KML file as a data source|
|`AddKmzFile`|`string` filePath|adds a KMZ file as a data source|
|`AddCoordinates`||adds a collection of `Point` objects as a data source|
||`string` collectionName|a name for the collection|
||`IEnumerable<Point>` coordinates|the points to add as a data source|

Gpx, kml and kmz files can contain multiple tracks or routes. These are preserved during the snapping process, except to the extent you choose to consolidate adjacent routes using filters.

[return to configuration](overview.md#configuration-via-extension-methods)

[return to overview](overview.md#j4jsoftwaregeoprocessor-overview)
