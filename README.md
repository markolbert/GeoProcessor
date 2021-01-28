# GeoProcessor
A C# Net5 library, command line app and Windows desktop app for processing vehicle 
geolocation files, snapping tracks/routes to roadways using the Bing or Google online
snap-to-route processors (**note:** you'll need an account with Bing or Google to
access those).

[![Nuget](https://img.shields.io/nuget/v/J4JSoftware.GeoProcessor?style=flat-square)](https://www.nuget.org/packages/J4JSoftware.GeoProcessor/)
### TL;DR
- The console app should compile and run anywhere Net5 is supported...but 
I've only tested it on Windows 10.

<img src="docs/assets/tldr-console.png" width="100%" align="middle" />

Configuration values can be specified in configuration files or, in some
cases, from the command line. Anything that's required and missing from
the configuration files will be prompted for at the console (you can also
force the app to confirm configuration values through the console).

- The Windows desktop app should run on any Windows system with Net5 installed...
but I've only tested it on Windows 10.

- You will need a Bing or Google API key for the library and program to
work.

### Table of Contents

- Command line app
  - [Command line options](docs/cmdline.md)
  - [Configuration file options](docs/config.md)
- Windows desktop app
  - [Configuration](docs/win-config.md)
  - [Running the app](docs/win-running-the-app.md)
- [The library](docs/library.md)
- [Obtaining API Keys](docs/apikeys.md)
