param(
    [Parameter(Mandatory=$true)]
    [string]$RepoName,
    [string]$OrgName,
    [string]$ProjectDir = ".",
    [string]$Name = "Release_x64",
    [string]$Configuration = "Release",
    [string]$Arch = "x64"
)
$ErrorActionPreference = "Stop"
$PSNativeCommandUseErrorActionPreference = $true

Write-Output "No tests yet"
