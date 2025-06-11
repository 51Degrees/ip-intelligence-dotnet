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
    [string]$ExamplesRepo = "ip-intelligence-dotnet-examples",
    [hashtable]$Keys
)
$ErrorActionPreference = "Stop"
$PSNativeCommandUseErrorActionPreference = $true
Set-StrictMode -Version 1.0

# If Version is not provided, the script is running in a workflow that doesn't build packages and the integration tests wil lbe skipped
if (!$Version) {
    Write-Host "Skipping integration tests"
    exit 0
}

Write-Debug "env:IPINTELLIGENCEDATAFILE = <$($env:IPINTELLIGENCEDATAFILE)>"

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

Write-Output "`n------- SETUP ENVIRONMENT BEGIN -------`n"

$SetupArgs = @{
    OrgName = $OrgName
    GitHubUser = $GitHubUser
    RepoName = $ExamplesRepo
    Name = $Name
    Arch = $Arch
    Configuration = $Configuration
    BuildMethod = $BuildMethod
    Keys = $Keys
}
& ./$ExamplesRepo/ci/setup-environment.ps1 @SetupArgs -Debug

Write-Output "`n------- SETUP ENVIRONMENT END -------`n"

Write-Debug "env:IPINTELLIGENCEDATAFILE = <$($env:IPINTELLIGENCEDATAFILE)>"

Write-Output "`n------- PACKAGE REPLACEMENT BEGIN -------`n"

Push-Location $ExamplesRepo
try {
    Write-Host "Restoring $ExamplesRepo..."
    dotnet restore
    foreach ($NextProject in (Get-ChildItem -Recurse -File -Filter '*.csproj')) {
        $NextProjectPath = $NextProject.FullName
        try {
            $ErrorActionPreference = "Continue"
            $PackagesRaw = (dotnet list $NextProjectPath package --format json)
        } finally {
            $ErrorActionPreference = "Stop"
        }
        if ($LASTEXITCODE -ne 0) {
            Write-Warning "--- RAW OUTPUT START ---"
            Write-Warning ($PackagesRaw -Join "`n")
            Write-Warning "--- RAW OUTPUT END ---"
            Write-Error "LASTEXITCODE = $LASTEXITCODE"
        }
        $PackagesNow = ($PackagesRaw | ConvertFrom-Json)
        $ToRemove = $PackagesNow.Projects | ForEach-Object {
            $_.Frameworks
        } | ForEach-Object {
            $_.TopLevelPackages
        } | ForEach-Object {
            $_.id
        } | Where-Object {
            $_.StartsWith("FiftyOne.IpIntelligence") 
        }
        foreach ($NextToRemove in $ToRemove) {
            Write-Output "Removing $NextToRemove..."
            dotnet remove $NextProjectPath package $NextToRemove
        }

        Write-Output "Adding the new packages..."
        dotnet add $NextProject package "FiftyOne.IpIntelligence" --version $Version
    }
    dotnet restore
} finally {
    Pop-Location
}

Write-Output "`n------- PACKAGE REPLACEMENT END -------`n"

Write-Output "`n------- RUN INTEGRATION TESTS BEGIN -------`n"

$BuildTestsArgs = @{
    RepoName = $ExamplesRepo
    ProjectDir = $ProjectDir
    Name = $Name
    Configuration = $Configuration
    Arch = $Arch
    BuildMethod = $BuildMethod
}
& ./$ExamplesRepo/ci/build-project.ps1 @BuildTestsArgs

$RunTestsArgs = $BuildTestsArgs + @{
    OutputFolder = "integration"
}
try {
    $ErrorActionPreference = "Continue"
    & ./$ExamplesRepo/ci/run-unit-tests.ps1 @RunTestsArgs -Debug
} finally {
    $ErrorActionPreference = "Stop"
}

Write-Output "`n------- RUN INTEGRATION TESTS END -------`n"

Copy-Item $ExamplesRepo/test-results $RepoName -Recurse

exit $LASTEXITCODE
