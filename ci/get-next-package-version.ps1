param (
    [Parameter(Mandatory)][string]$RepoName,
    [string]$VariableName = "Version"
)
./dotnet/get-next-package-version.ps1 -RepoName:$RepoName -VariableName:$VariableName
