# GeoProcessorApp

A C# Net5 command line app for processing vehicle geolocation files, snapping tracks/routes to roadways using the Bing or Google online snap-to-route processors (**note:** you'll need an account with Bing or Google to access those).

The library and apps are licensed under the GNU GPL-v3.0 (or later) license.

For more details consult the [github documentation](https://github.com/markolbert/GeoProcessor).

Certain options can be configured from either the command line or from a configuration file.

|Key            |Explanation    |Default    |
|---------------|---------------|-----------|
|-i, --inputFile|the input file to process|*none*|
|-n, --defaultName|default name for a route if none is specified in the input file|Unnamed Route|
|-o, --outputFile|the output file to create file type (e.g., kmz) will be derived from input file extension but can be overriden|*none*|
|-t, --outputType|the type of output file to create. Must be one of *kml* or *kmz*| kml|
|-p, --snapProcessor|the snap-to-route processor to use. Must be one of *Bing* or *Google*| Undefined |
|-r, --runInteractive|force entry of all command line parameters regardless of whether or not they're specified in config file| false (not set)|
|-k, --storeApiKey|prompt user for a snap-to-route processor's API key and store it, encrypted, in the app's user configuration| false (not set)|

File names should include the path to the file, either relative to the current directory or the full path.

File names or default route names containing spaces need to be wrapped in quotes (").
