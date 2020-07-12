[![Encode Your Data With Emoji](docfx/images/banner@2x.png)][docsite]

[![.NET Core](https://github.com/abock/dotnet-ecoji/workflows/.NET%20Core/badge.svg)](https://github.com/abock/dotnet-ecoji/actions?query=workflow%3A%22.NET+Core%22)
[![NuGet Badge](https://buildstats.info/nuget/dotnet-ecoji)](https://www.nuget.org/packages/dotnet-ecoji/)
[![License](https://img.shields.io/badge/license-MIT%20License-blue.svg)](LICENSE)

Encode and decode data using emoji in .NET. Like base64, except base1024, and uses an emoji alphabet. Ecoji .NET is a .NET Standard 2.0 implementation of [Keith Turner's Ecoji Standard](https://github.com/keith-turner/ecoji). In addition to the tiny `netstandard2.0` library, a .NET Core command line tool is available.

Visit [ecoji.io](https://ecoji.io) to try Ecoji in your browser.

## Read the Docs

For code and command line tool examples, API documentation, and a little more,
[visit the full documentation][docsite].

## Use It In Your App

```bash
$ dotnet add package ecoji
```

## Install the Command Line Tool

```bash
$ dotnet tool install -g dotnet-ecoji
```

## Build & Contribute

* [Install .NET Core 3.1](https://dotnet.microsoft.com/download)
* [Clone The Repo](https://github.com/abock/dotnet-ecoji)
* From the root of the checkout you may:
  * Run `dotnet build` to build everything
  * Run `dotnet run -p src/Ecoji.CommandLineTool` to run the command line tool
  * Run `dotnet test` to run the unit tests
* Please file issues and pull requests if you feel so compelled! 🍿


[docsite]: https://ecoji.dev
