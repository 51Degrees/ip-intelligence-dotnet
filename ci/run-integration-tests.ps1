param(
    [Parameter(Mandatory=$true)]
    [string]$RepoName,
    [string]$ProjectDir = ".",
    [string]$Name = "Release_x64",
    [string]$Configuration = "Release",
    [string]$Arch = "x64",
    [string]$Version,
    [string]$BuildMethod,
    [string]$OrgName,
    [string]$Branch = "main"
)

Write-Output "No tests yet"
