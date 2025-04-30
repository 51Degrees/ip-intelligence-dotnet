[CmdletBinding()]
param(
    [Parameter(Mandatory=$true)]
    [string]$OrgName,
    [string]$GitHubUser = "Automation51D",
    [string]$ProjectDir = ".",
    [string]$Name,
    [Parameter(Mandatory=$true)]
    [string]$RepoName
)

./dotnet/add-nuget-source.ps1 `
    -Source "https://nuget.pkg.github.com/$OrgName/index.json" `
    -UserName $GitHubUser `
    -Key $env:GITHUB_TOKEN

if ($LASTEXITCODE -eq 0) {
    ./dotnet/run-update-dependencies.ps1 -RepoName $RepoName -ProjectDir $ProjectDir -Name $Name -IncludePrerelease
}

if ($LASTEXITCODE -ne 0) {
    Write-Warning "LASTEXITCODE = $LASTEXITCODE"
}

exit $LASTEXITCODE
