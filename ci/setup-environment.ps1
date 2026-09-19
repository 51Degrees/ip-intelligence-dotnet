param(
    [Parameter(Mandatory=$true)]
    [string]$RepoName,
    [string]$ProjectDir = ".",
    [string]$Name = "Release_x64",
    [string]$Arch = "x64",
    [string]$Configuration = "Release",
    [string]$BuildMethod,
    [hashtable]$Keys
)
$ErrorActionPreference = "Stop"
$PSNativeCommandUseErrorActionPreference = $true

$RepoPath = [IO.Path]::Combine($pwd, $RepoName)

if ($BuildMethod -ne "dotnet") {

    # Setup the MSBuild environment if it is required.
    ./environments/setup-msbuild.ps1
    ./environments/setup-vstest.ps1
}

if ($IsLinux) {
    # Shared, because the same call in five repositories carried the same
    # fault: these packages do not exist on arm64, so asking for them on
    # an ARM runner fails the step. See common-ci environments/README.md.
    ./environments/setup-multilib.ps1

}

$env:_51DEGREES_IPI_PATH = [IO.Path]::Combine($RepoPath, "FiftyOne.IpIntelligence.Engine.OnPremise", "ip-intelligence-cxx", "ip-intelligence-data", "51Degrees-EnterpriseIpiV41.ipi")
