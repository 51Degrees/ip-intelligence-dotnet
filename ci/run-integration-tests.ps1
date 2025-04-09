param(
    [Parameter(Mandatory)][string]$RepoName,
    [Parameter(Mandatory)][string]$OrgName,
    [string]$GitHubUser = "Automation51D",
    [Parameter(Mandatory)][string]$DeviceDetection,
    [Parameter(Mandatory)][string]$DeviceDetectionUrl,
    [string]$ProjectDir = ".",
    [string]$Name = "Release_x64",
    [string]$Configuration = "Release",
    [string]$Arch = "x64",
    [string]$Version,
    [string]$BuildMethod = "dotnet",
    [string]$Branch = "main",
    [string]$ExamplesBranch = "main",
    [string]$ExamplesRepo = "ip-intelligence-dotnet-examples"
)
$ErrorActionPreference = "Stop"
$PSNativeCommandUseErrorActionPreference = $true
Set-StrictMode -Version 1.0

# If Version is not provided, the script is running in a workflow that doesn't build packages and the integration tests wil lbe skipped
if (!$Version) {
    Write-Host "Skipping integration tests"
    exit 0
}

Write-Host "Fetching examples..."
./steps/clone-repo.ps1 -RepoName $ExamplesRepo -OrgName $OrgName -Branch $ExamplesBranch
& "./$ExamplesRepo/ci/fetch-assets.ps1" -RepoName $ExamplesRepo -DeviceDetection $DeviceDetection -DeviceDetectionUrl $DeviceDetectionUrl

Push-Location package
try {
    $localFeed = New-Item -ItemType Directory -Force "$HOME/.nuget/packages"
    dotnet nuget add source $localFeed
    dotnet nuget push -s $localFeed '*.nupkg'
} finally {
    Pop-Location
}


./dotnet/add-nuget-source.ps1 `
    -Source "https://nuget.pkg.github.com/$OrgName/index.json" `
    -UserName $GitHubUser `
    -Key $env:GITHUB_TOKEN

Push-Location $ExamplesRepo
try {
    Write-Host "Restoring $ExamplesRepo..."
    foreach ($NextProject in (Get-ChildItem -Recurse -File -Filter '*.csproj')) {
        $PackagesRaw = (dotnet list $NextProject.FullName package --format json)
        $PackagesNow = ($PackagesRaw | ConvertFrom-Json)
        $ToRemove = $PackagesNow.Projects[0].Frameworks | ForEach-Object {
            $_.TopLevelPackages
        } | Select-Object id | ForEach-Object {
            $_.id
        } Where-Object {
            $_.StartsWith("FiftyOne.IpIntelligence") 
        }
        foreach ($NextToRemove in $ToRemove) {
            Write-Output "Removing $NextToRemove..."
            dotnet package remove $NextToRemove --project $NextProject.FullName
        }

        Write-Output "Adding the new packages..."
        dotnet add $NextProject package "FiftyOne.IpIntelligence" --version $Version
    }
    dotnet restore
} finally {
    Pop-Location
}

./dotnet/run-integration-tests.ps1 -RepoName $ExamplesRepo -ProjectDir $ProjectDir -Name $Name -Configuration $Configuration -Arch $Arch -BuildMethod $BuildMethod -DirNameFormatForDotnet '*' -DirNameFormatForNotDotnet "*" -Filter ".*\.sln"
