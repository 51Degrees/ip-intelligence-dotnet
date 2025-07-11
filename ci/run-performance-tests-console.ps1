[CmdletBinding()]
param(
    [Parameter(Mandatory=$true)]
    [string]$RepoName,
    [string]$OrgName,
    [string]$Name = "Release_x64",
    [string]$Configuration = "Release",
    [string]$Arch = "x64"
)

$RepoPath = [IO.Path]::Combine($pwd, $RepoName)
$PerfResultsFile = [IO.Path]::Combine($RepoPath, "test-results", "performance-summary", "results_$Name.json")
$EvidenceFiles = [IO.Path]::Combine($pwd, $RepoName,"FiftyOne.IpIntelligence.Engine.OnPremise", "ip-intelligence-cxx", "ip-intelligence-data")
$ExamplesRepoName = "ip-intelligence-dotnet-examples"
$ExamplesRepoPath = [IO.Path]::Combine($pwd, $ExamplesRepoName)
$IpIntelligenceProject = [IO.Path]::Combine($RepoPath, "FiftyOne.IpIntelligence", "FiftyOne.IpIntelligence.csproj")
$PerfProject = [IO.Path]::Combine($ExamplesRepoPath, "Examples", "OnPremise", "Performance-Console")

Write-Output "Entering '$RepoPath'"
Push-Location $RepoPath

try {
    # Create the output directories if they don't already exist.
    if ($(Test-Path -Path "test-results") -eq  $False) {
        mkdir test-results
    }
    if ($(Test-Path -Path "test-results/performance-summary") -eq  $False) {
        mkdir test-results/performance-summary
    }
}
finally {
    Write-Output "Leaving '$RepoPath'"
    Pop-Location
}

if ($(Test-Path -Path $ExamplesRepoName) -eq $False) {
    Write-Output "Cloning '$ExamplesRepoName'"
    ./steps/clone-repo.ps1 -RepoName $ExamplesRepoName -OrgName $OrgName
}

Write-Output "Moving enterprise IPI file"
$EnterpriseFile = [IO.Path]::Combine($EvidenceFiles, "51Degrees-EnterpriseIpiV41.ipi") 
Copy-Item $EnterpriseFile "ip-intelligence-dotnet-examples/ip-intelligence-data/51Degrees-EnterpriseIpiV41.ipi"

Write-Output "Moving evidence file"
$EvidenceFile = [IO.Path]::Combine($EvidenceFiles, "evidence.yml")
Copy-Item $EvidenceFile "ip-intelligence-dotnet-examples/ip-intelligence-data/evidence.yml"

$ExamplesProject = [IO.Path]::Combine($ExamplesRepoPath, "Examples", "FiftyOne.IpIntelligence.Examples")

# Update the dependency in the examples project to point to the newly bulit package
Write-Output "Entering '$ExamplesProject'"
Push-Location $ExamplesProject
try{
    # Change the dependency version to the locally build Nuget package
    Write-Output "Replacing the IpIntelligence package with a local reference."
    dotnet remove package "FiftyOne.IpIntelligence"
    dotnet add reference $IpIntelligenceProject
}
finally{
    Write-Output "Leaving '$ExamplesProject'"
    Pop-Location
}

Write-Output "Running performance example with config $Configuration|$Arch"
Write-Output "Entering '$PerfProject' folder"
Push-Location "$PerfProject"
try {
    $RunConfig = "Debug"
    if ($Configuration.Contains("Release")) {
        $RunConfig = "Release"
    }

    dotnet build -c $RunConfig /p:Platform=$Arch /p:OutDir=output /p:BuiltOnCI=true
    if ($LASTEXITCODE -ne 0) {
        Write-Error "LASTEXITCODE = $LASTEXITCODE"
    }

    Write-Debug "Entering output..."
    Push-Location "output"
    try {
        dotnet FiftyOne.IpIntelligence.Examples.OnPremise.Performance.dll -d $EnterpriseFile -a $EvidenceFile -j summary.json
    }
    finally {
        Pop-Location
    }
    
    if ($LASTEXITCODE -ne 0) {
        Write-Error "LASTEXITCODE = $LASTEXITCODE"
    }

    # Write out the results for comparison
    Write-Output "Writing performance test results"
    $Results = Get-Content ./output/summary.json | ConvertFrom-Json
    $PerfResults = "{
        'HigherIsBetter': {
            'DetectionsPerSecond': $($Results.MaxPerformance.DetectionsPerSecond)
        },
        'LowerIsBetter': {
            'MsPerDetection': $($Results.MaxPerformance.MsPerDetection)
        }
    }"
    Write-Debug "--- RESULTS BEGIN ---"
    Write-Debug $PerfResults
    Write-Debug "--- RESULTS END ---"
    Write-Debug "Saving to $PerfResultsFile..."
    Set-Content -Value $PerfResults -Path $PerfResultsFile
}
finally {
    Write-Output "Leaving '$PerfProject'"
    Pop-Location
}
