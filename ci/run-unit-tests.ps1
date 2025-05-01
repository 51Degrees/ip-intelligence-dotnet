param(
    [Parameter(Mandatory=$true)]
    [string]$RepoName,
    [string]$ProjectDir = ".",
    [string]$Name = "Release_x64",
    [string]$Configuration = "Release",
    [string]$Arch = "x64",
    [string]$BuildMethod = "dotnet"
)
$ErrorActionPreference = "Stop"
$PSNativeCommandUseErrorActionPreference = $true

Write-Output "Looking for OnPremise.Native.dll`n"
Get-ChildItem -Recurse -Include "*.OnPremise.Native.dll" `
| Select-Object -ExpandProperty FullName `
| Resolve-Path -Relative -RelativeBasePath .

./dotnet/run-unit-tests.ps1 `
    -RepoName $RepoName `
    -ProjectDir $ProjectDir `
    -Name $Name `
    -Configuration $Configuration `
    -Arch $Arch `
    -BuildMethod $BuildMethod `
    -Filter ".*Tests(|\.Core|\.Web)\.dll$"

exit $LASTEXITCODE
