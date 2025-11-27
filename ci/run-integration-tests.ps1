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

$updateExitCode = $null
function Update-CsprojRefs {
    param (
        [Parameter(Mandatory)]
        [string]$CsprojPath
    )
    
    $PackagesRaw = (dotnet list $CsprojPath package --format json)
    $script:updateExitCode = $LASTEXITCODE
    if ($LASTEXITCODE -ne 0) {
        Write-Warning "--- RAW OUTPUT START ---"
        Write-Warning ($PackagesRaw -Join "`n")
        Write-Warning "--- RAW OUTPUT END ---"
        Write-Warning "^ LASTEXITCODE (dotnet list) = $LASTEXITCODE"
        $LASTEXITCODE_DOTNET_LIST = $LASTEXITCODE
        dotnet restore
        Write-Warning "LASTEXITCODE (dotnet restore) = $LASTEXITCODE"
        Write-Warning "LASTEXITCODE (dotnet list) = $LASTEXITCODE_DOTNET_LIST"
        return
    }
    $PackagesNow = ($PackagesRaw | ConvertFrom-Json)
    $ToRemove = $PackagesNow.Projects | ForEach-Object {
        $_.Frameworks
    } | ForEach-Object {
        $_.TopLevelPackages
    } | Where-Object { $_ } | ForEach-Object {
        $_.id
    } | Where-Object {
        $_ -and $_.StartsWith("FiftyOne.IpIntelligence")
    }
    foreach ($NextToRemove in $ToRemove) {
        Write-Output "Removing $NextToRemove..."
        dotnet remove $CsprojPath package $NextToRemove
        $script:updateExitCode = $LASTEXITCODE
        if ($LASTEXITCODE -ne 0) {
            Write-Error "LASTEXITCODE (dotnet remove) = $LASTEXITCODE"
        }
    }

    Write-Output "Adding the new packages..."
    dotnet add $CsprojPath package "FiftyOne.IpIntelligence" --version $Version
    $script:updateExitCode = $LASTEXITCODE
    if ($LASTEXITCODE -ne 0) {
        Write-Error "LASTEXITCODE (dotnet add) = $LASTEXITCODE"
    }
}

Push-Location $ExamplesRepo
try {
    Write-Host "Restoring $ExamplesRepo..."
    dotnet restore
    $CsprojPaths = @(
        "Examples/OnPremise/UpdateDataFile-Console/UpdateDataFile-Console.csproj",
        "Examples/OnPremise/Mixed/GettingStarted-Web/GettingStarted-Web.csproj",
        "Examples/OnPremise/GettingStarted-Console/GettingStarted-Console.csproj",
        "Examples/OnPremise/OfflineProcessing-Console/OfflineProcessing-Console.csproj",
        "Examples/OnPremise/GettingStarted-Web/GettingStarted-Web.csproj",
        "Tests/FiftyOne.IpIntelligence.Example.Tests.Cloud/FiftyOne.IpIntelligence.Example.Tests.Cloud.csproj",
        "Tests/FiftyOne.IpIntelligence.Example.Tests.OnPremise/FiftyOne.IpIntelligence.Example.Tests.OnPremise.csproj"
    ) + (Get-ChildItem -Recurse -File -Filter '*.csproj' | Select-Object -ExpandProperty FullName)
    $nextRound = $CsprojPaths

    while ($nextRound.Count -gt 0) {
        $topLevelSuccess = $false
        $currentRound = $nextRound
        $nextRound = @()

        foreach ($NextProjectPath in $currentRound) {
            $updateExitCode = $null
            try {
                $ErrorActionPreference = "Continue"
                Update-CsprojRefs -CsprojPath $NextProjectPath
                Write-Warning "updateExitCode ($NextProjectPath) = $updateExitCode"
            } finally {
                $ErrorActionPreference = "Stop"
            }

            if ($updateExitCode -eq 0) {
                Write-Warning "Success: $NextProjectPath"
                $topLevelSuccess = $true
            } else {
                Write-Warning "Retrying: $NextProjectPath"
                $nextRound += $NextProjectPath
            }
        }

        if (-not $topLevelSuccess) {
            Write-Warning "No progress made. Breaking."
            Write-Warning "Remaining items:"
            foreach ($NextProjectPath in $nextRound) {
                Write-Output $NextProjectPath
            }
            Write-Error "Update failed. Remaining items: $($nextRound.Count)"
            break
        }
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
