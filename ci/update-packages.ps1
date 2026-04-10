param(
    [Parameter(Mandatory)][string]$OrgName,
    [Parameter(Mandatory)][string]$RepoName,
    [string]$GitHubUser = "Automation51D"
)

./dotnet/add-nuget-source.ps1 `
    -Source "https://nuget.pkg.github.com/$OrgName/index.json" `
    -UserName $GitHubUser `
    -Key $env:GITHUB_TOKEN

./dotnet/outdated.ps1 -RepoName:$RepoName
