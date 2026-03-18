param(
    [Parameter(Mandatory=$true)]
    [string]$RepoName = "ip-intelligence-dotnet",
    [string]$ProjectDir = ".",
    [string]$Name = "Release_x64",
    [string]$Configuration = "Release",
    [string]$Arch = "x64",
    [string]$BuildMethod = "msbuild"
)
$ErrorActionPreference = "Stop"
$PSNativeCommandUseErrorActionPreference = $true

$BuildArgs = @{
    RepoName = $RepoName
    ProjectDir = $ProjectDir
    Name = $Name
    Configuration = $Configuration
    Arch = $Arch
}

if ($BuildMethod -eq "dotnet"){
    ./dotnet/build-project-core.ps1 @BuildArgs
}
else{
    ./dotnet/build-project-framework.ps1 @BuildArgs
}

exit $LASTEXITCODE