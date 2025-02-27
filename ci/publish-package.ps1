
param(
    [string]$ProjectDir = ".",
    [string]$Name = "Release",
    [Parameter(Mandatory=$true)]
    [string]$OrgName,
    [Parameter(Mandatory=$true)]
    [string]$ApiKey
)
$ErrorActionPreference = "Stop"
$PSNativeCommandUseErrorActionPreference = $true

./dotnet/publish-package-github.ps1 -RepoName $RepoName -OrgName $OrgName -ApiKey $env:GITHUB_TOKEN

exit $LASTEXITCODE
