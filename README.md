# 51Degrees IP Intelligence Engines

![51Degrees](https://51degrees.com/img/logo.png?utm_source=github&utm_medium=repository&utm_content=readme_main&utm_campaign=dotnet-open-source "Data rewards the curious") **Pipeline API**

[Developer Documentation](https://51degrees.com/ip-intelligence-dotnet/4.1/index.html "developer documentation")

## Introduction

This repository contains the IP intelligence engines for the .NET implementation of the Pipeline API.

## Pre-requesites

Visual Studio 2019 or later is recommended. Although Visual Studio Code can be used for working with most of the projects.

The core IP intelligence projects are written in C and C++.
The Pipeline engines are written in C# and target .NET Standard 2.0.3.
Example and test projects mostly target .NET Core 3.1 though in some cases, projects are available targeting other frameworks.

## Solutions and projects

- **FiftyOne.IpIntelligence** - Ip intelligence engines and related projects.
  - **FiftyOne.IpIntelligence.Engine.OnPremise** - .NET implementation of the IP intelligence on-premise engine.
  - **FiftyOne.IpIntelligence.Shared** - Shared classes used by the IP intelligence engines.
  - **FiftyOne.IpIntelligence** - Contains IP intelligence engine builders.
  - **FiftyOne.IpIntelligence.Cloud** - A .NET engine which retrieves IP intelligence results by consuming the 51Degrees cloud service. This can be swapped out with either the on-premise engine seamlessly.
  
## Installation

You can either reference the projects in this repository or you can reference the [NuGet][nuget] packages in your project:

```
Install-Package FiftyOne.IpIntelligence
```

## Examples

Examples can be found in the `Examples/` folder, there are separate sources for cloud and on-premise implementations and solutions for .NET Core and .NET Framework. See below for a list of examples.

### IP Intelligence

|Example|Description|Implementations|
|-------|-----------|---------------|
|ConfigureFromFile|This example shows how to build a Pipeline from a configuration file.|On-premise|
|GettingStarted|This example uses 51Degrees IP intelligence to determine in which country a given IP address is located.|On-premise / Cloud|
|Metadata|This example shows how to get all the properties from the IP intelligence engine and print their information details.|On-premise|
|Performance|This example demonstrates the performance of the maximum performance IP intelligence configuration profile.|On-premise|
|GetAllProperties|This example demonstrates how to iterate through all properties in a response.|Cloud|

### Web Integrations

These examples show how to integrate the Pipeline API into a simple web app.

|Example|Description|
|-------|-----------|
|Cloud IpIntelligenceWebDemo NetCore 3.1|This example demonstrates how to use the 51Degrees cloud to perform IP intelligence from an ASP.NET Core 3.1 web application|
|IpIntelligenceWebDemo NetCore 2.1|This example demonstrates how to use on-premise IP intelligence from an ASP.NET Core 2.1 web application|
|IpIntelligenceWebDemo NetCore 3.1|This example demonstrates how to use on-premise IP intelligence from an ASP.NET Core 3.1 web application|

## Tests

Tests can be found in the `Tests/` folder. These can all be run from within Visual Studio or by using the `dotnet` command line tool. 

## Project documentation

For complete documentation on the Pipeline API and associated engines, see the [51Degrees documentation site][Documentation].

[Documentation]: https://51degrees.com/documentation/4.1/index.html
[nuget]: https://www.nuget.org/packages/FiftyOne.IpIntelligence/
