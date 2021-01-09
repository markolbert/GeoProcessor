## Configuration Files
The app has a general configuration file, in its program directory, for
configuring the logging system, specifying how close raw coordinates should
be to each other to be merged, etc. It's a simple JSON file that looks
like this:
```json
{
  "Processors": {
    "Distance": {
      "MaxDistanceMultiplier": 3,
      "MaxPointsPerRequest": -1,
      "MaxSeparation": "100 ft"
    },
    "Bing": {
      "MaxPointsPerRequest": 100,
      "MaxSeparation": "2 km"
    },
    "Google": {
      "MaxPointsPerRequest": 100,
      "MaxSeparation": "2 km"
    }
  },
  "Logging": {
    "EventElements": "None",
    "SourceRootPath": "C:\\Programming\\KMLProcessor\\",
    "Channels": {
      "Debug": {
        "MinimumLevel": "Information"
      },
      "Console": {
        "MinimumLevel": "Information"
      }
    }
  }
}
```
There's also a user configuration file which stores encrypted snap-to-route
API keys, if those are defined by the user (i.e., not simply entered 
each time the program is run). It looks like this:
```json
{
  "APIKeys": {
    "Bing": {
      "EncryptedValue": "CfDJ8DZjcBUs1sNEu98tAdCI1mAkm...omitted for brevity"
    },
    "Google": {
      "EncryptedValue": "CfDJ8DZjcBUs1sNEu98tAdCI1mCS2...omitted for brevity"
    }
  }
}
```
On Windows it's located at <br/>
*C:\Users\\[username]\AppData\Local\J4JSoftware\GeoProcessor*.

The `MaxSeparation` text values need to be [convertible into `Distance`
objects](distance.md).