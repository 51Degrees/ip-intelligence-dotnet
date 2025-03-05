
param (
    [Parameter(Mandatory=$true)]
    [string]$VariableName,
    [string]$RepoName
)
$ErrorActionPreference = "Stop"
$PSNativeCommandUseErrorActionPreference = $true

./dotnet/get-next-package-version.ps1 -RepoName $RepoName -VariableName $VariableName

exit $LASTEXITCODE