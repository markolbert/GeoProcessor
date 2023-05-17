# Specifying Outputs

The library can export snapped routes to several geolocation formats.

|Extension Method|Arguments|Comments|
|----------------|---------|--------|
|`ExportToGpx`|`string` filePath|exports to a GPX file|
||`Distance?` maxGap = null|restricts the exported points from being no closer than the provided value. This enables you to reduce the size of a file at the price of less route "smoothness". Defaults to 0, meaning all points will be exported.|
|`ExportToKml`|`string` filePath|exports to a KML file|
||`Distance?` maxGap = null|restricts the exported points from being no closer than the provided value. This enables you to reduce the size of a file at the price of less route "smoothness". Defaults to 0, meaning all points will be exported.|
|`ExportToKmz`|`string` filePath|exports to a KMZ file|
||`Distance?` maxGap = null|restricts the exported points from being no closer than the provided value. This enables you to reduce the size of a file at the price of less route "smoothness". Defaults to 0, meaning all points will be exported.|

[return to configuration](overview.md#configuration-via-extension-methods)

[return to overview](overview.md#j4jsoftwaregeoprocessor-overview)
