# GeoProcessor
A C# Net5 library and app for processing vehicle geolocation files,
snapping tracks/routes to roadways using the Bing or Google online
snap-to-route processors (**note:** you'll need an account with Bing or Google to
access those).

[![Nuget](https://img.shields.io/nuget/v/J4JSoftware.GeoProcessor?style=flat-square)](https://www.nuget.org/packages/J4JSoftware.GeoProcessor/)
### TL;DR
The app should compile and run anywhere Net5 is supported...but 
I've only tested it on Windows 10.

![tl;dr console image](docs/assets/tldr-console.png)

Configuration values can be specified in configuration files or, in some
cases, from the command line. Anything that's required and missing from
the configuration files will be prompted for at the console (you can also
force the app to confirm configuration values through the console).

You will need a Bing or Google API key for the library and program to
work.

### Table of Contents

- [Command line options](docs/cmdline.md)
- [Configuration file options](docs/config.md)
- [Obtaining API Keys](docs/apikeys.md)