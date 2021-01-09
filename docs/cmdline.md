## Command Line Options
Certain options can be configured from either the command line or
from a [configuration file](config.md).

|Key        |Explanation|Default|
|-----------|---------------|-----------|
|-i<br/>--inputFile|the input file to process|*none*|
|-n<br/>--defaultName|default name for a route if none is specified in the input file|Unnamed Route|
|-o<br/>--outputFile|the output file to create<br/><br/>file type (e.g., kmz) will be derived from input file extension but can be overriden|*none*|
|-t<br/>--outputType|the type of output file to create. Must be one of *kml* or *kmz*| kml|
|-p<br/>--snapProcessor|the snap-to-route processor to use. Must be one of *Bing* or *Google*| Undefined |
|-r<br/>--runInteractive|force entry of all command line parameters regardless of whether or not they're specified in config file| false (not set)|
|-k<br/>--storeApiKey|prompt user for a snap-to-route processor's API key and store it, encrypted, in the app's user configuration| false (not set)|

File names should include the path to the file, either relative to
the current directory or the full path.

File names or default route names containing spaces need to be wrapped 
in quotes (").