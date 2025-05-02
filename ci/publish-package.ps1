param(
    [Parameter(Mandatory)][string]$RepoName,
    [Parameter(Mandatory)][string]$OrgName,
    [Parameter(Mandatory)][string]$ApiKey,
    [string]$ProjectDir = ".",
    [string]$Name = "Release"
)
$ErrorActionPreference = "Stop"
$PSNativeCommandUseErrorActionPreference = $true

# ./dotnet/publish-package-github.ps1 -RepoName $RepoName -OrgName $OrgName -ApiKey $env:GITHUB_TOKEN
./dotnet/publish-package-nuget.ps1 -RepoName $RepoName -ApiKey $ApiKey -ProjectDir $ProjectDir -Name Name
