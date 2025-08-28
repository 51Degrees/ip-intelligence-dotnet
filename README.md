# 51Degrees IP Intelligence Engines

![51Degrees](https://51degrees.com/img/logo.png?utm_source=github&utm_medium=repository&utm_content=readme_main&utm_campaign=dotnet-open-source "Data rewards the curious") **Pipeline API**

[Developer Documentation](https://51degrees.com/ip-intelligence-dotnet/4.1/index.html "developer documentation")

## Introduction

This repository contains the IP intelligence engines for the .NET implementation of the Pipeline API.

The
[specification](https://github.com/51Degrees/specifications/blob/main/ip-intelligence-specification/README.md)
is also available on GitHub and is recommended reading if you wish to understand
the concepts and design of this API.

## Dependencies

Visual Studio 2019 or later is recommended. Although Visual Studio Code can be used for working with most of the projects.

The core IP intelligence projects are written in C and C++.
The Pipeline engines are written in C# and target .NET Standard 2.0. Example and test
projects mostly target .NET 8.0 though in some cases, projects are available
targeting other frameworks.

For runtime dependencies, see our
[dependencies](https://51degrees.com/documentation/_info__dependencies.html)
page. The `ci/options.json` file lists the tested and packaged .NET versions
and operating systems automatic tests are performed with. The solution will
likely operate with other versions.

### Data

The API does detections using a local (on-premise) data file or cloud service (coming soon). 

#### On-Premise

In order to perform IP intelligence on-premise, you will need to use a
data file.

[ip-intelligence-data](https://github.com/51Degrees/ip-intelligence-data/) submodule repository instructs how to obtain a 'Lite' data file, otherwise [contact us](https://51degrees.com/contact-us) to obtain an 'Enterprise' data file.

#### Cloud (coming soon)

You will require [resource keys](https://51degrees.com/documentation/_info__resource_keys.html)
to use the Cloud API, as described on our website. Get resource keys from
our [configurator](https://configure.51degrees.com/), see our [documentation](https://51degrees.com/documentation/_concepts__configurator.html) on
how to use this.

## Solutions and projects

- **FiftyOne.IpIntelligence** - Ip intelligence engines and related projects.
  - **FiftyOne.IpIntelligence.Engine.OnPremise** - .NET implementation of the IP intelligence on-premise engine.
  - **FiftyOne.IpIntelligence.Shared** - Shared classes used by the IP intelligence engines.
  - **FiftyOne.IpIntelligence** - Contains IP intelligence engine builders.
  - **FiftyOne.IpIntelligence.Cloud** - A .NET engine which retrieves IP intelligence results by consuming the 51Degrees Cloud service. This can be swapped out with either the on-premise engine seamlessly.
  
## Installation

### Nuget

The easiest way to install is to use NuGet to add the reference to the package:

```pwsh
Install-Package FiftyOne.IpIntelligence
```

### Build from Source

IP Intelligence on-premise uses a native binary (i.e. compiled from C code to
target a specific platform/architecture). The NuGet package contains several
binaries for common platforms. However, in some cases, you'll need to build the
native binaries yourself for your target platform. This section explains how to
do this.

#### Pre-requisites

- Install C build tools:
  - Windows:
    - You will need either Visual Studio 2022 or the [C++ Build Tools](https://visualstudio.microsoft.com/visual-cpp-build-tools/) installed.
      - Minimum platform toolset version is `v143`
      - Minimum Windows SDK version is `10.0.18362.0`
    - Linux/MacOS:
      - `sudo apt-get install g++ make libatomic1`
- If you have not already done so, pull the git submodules that contain the
    native code:
  - `git submodule update --init --recursive`

Visual studio should now be able to build the native binaries as part of its
normal build process.

#### Packaging

You can package a project into NuGet `*.nupkg` file by running a command like:

```sh
dotnet pack [Project] -o "[PackagesFolder]" /p:PackageVersion=0.0.0 -c [Configuration] /p:Platform=[Architecture]
```

##### ⚠️ Notes on packaging `FiftyOne.IpIntelligence.Engine.OnPremise`

📝 Using `AnyCPU` might prevent the unmanaged (C++) code from being built into `.Native.dll` library. Use `x64`/`arm64` specifically.

📝 If creating cross-platform package from multiple native dlls, put all 4x `FiftyOne.IpIntelligence.Engine.OnPremise.Native.dll` into respective folders:

```text
../
    macos/
        arm64/
        x64/
    linux/
        x64/
    windows/
        x64/
```

and add to the packaging command:

```text
/p:BuiltOnCI=true
```

related CI scripts:

- `BuiltOnCI` var:
  - [https://github.com/51Degrees/common-ci/blob/main/dotnet/build-project-core.ps1]
  - [https://github.com/51Degrees/common-ci/blob/main/dotnet/build-package-nuget.ps1]
  - [https://github.com/51Degrees/common-ci/blob/main/dotnet/build-project-framework.ps1]
  - [https://github.com/51Degrees/ip-intelligence-dotnet/blob/main/ci/run-performance-tests-console.ps1]
- Copying native binaries:
  - [https://github.com/51Degrees/ip-intelligence-dotnet/blob/main/ci/build-package.ps1]

#### Strong naming

We currently do not [strong name](https://learn.microsoft.com/en-us/dotnet/standard/library-guidance/strong-naming#create-strong-named-net-libraries) assemblies due to downsides for developers. The main of which is that .NET Framework on Windows enables strict loading of assemblies once an assembly is strong named. A strong-named assembly reference must exactly match the version of the loaded assembly, forcing developers to configure binding redirects when using the assembly.

If it is absolutely critical for your use case to integrate a strong-named assembly - please create a feature request [issue](https://github.com/51Degrees/ip-intelligence-dotnet/issues/new).

## Examples

Examples can be found in
[ip-intelligence-dotnet-examples](https://github.com/51Degrees/ip-intelligence-dotnet-examples)
repository.

## Tests

Tests can be found in the `Tests/` folder. These can all be run from within
Visual Studio or by using the `dotnet test` command line tool.

Some tests require additional resources to run. These will either fail or return
an 'inconclusive' result if these resources are not provided.

- Some tests require an 'Enterprise' data file. This can be obtained by [purchasing a license](https://51degrees.com/pricing).
- Tests using the Cloud service require resource keys with specific properties. A [license](https://51degrees.com/pricing) is required in order to access some properties.

## Project documentation

For complete documentation on the Pipeline API and associated engines, see the
[51Degrees documentation site](https://51degrees.com/documentation/index.html).
