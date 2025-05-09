[CmdletBinding()]
param(
    [Parameter(Mandatory=$true)]
    [string]$RepoName,
    [string]$ProjectDir = ".",
    [string]$Name = "Release_x64",
    [string]$Configuration = "Release",
    [string]$Arch = "x64"
)


$RepoPath = [IO.Path]::Combine($pwd, $RepoName)
$PerfPath = [IO.Path]::Combine($RepoPath, "performance-tests")

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

# workaround, see https://github.com/actions/runner-images/issues/8598
$env:PATH = $env:PATH -replace "C:\\Strawberry\\c\\bin;"

Write-Output "Setting Data File for testing"

$SettingsFile = [IO.Path]::Combine($RepoPath, "performance-tests", "appsettings.json") 

Write-Debug "Reading $SettingsFile..."
# Read the contents of the appsettings.json file
$json = (Get-Content -Path $SettingsFile) -replace '^\s*//.*' | Out-String | ConvertFrom-Json

Write-Debug "Reading env var..."
$NewDataFilePath = $env:IPINTELLIGENCEDATAFILE
if (($null -eq $NewDataFilePath) -or (("" -eq "$NewDataFilePath"))) {
    Write-Error "NewDataFilePath is not set."
}
if (!(Test-Path -Path $NewDataFilePath -PathType Leaf)) {
    Write-Error "File not found at '$NewDataFilePath'."
}

Write-Debug "Patching JSON..."
# Update the "DataFile" value
$json.PipelineOptions.Elements[0].BuildParameters.DataFile = $NewDataFilePath

# Convert the updated JSON object back to a string
$jsonString = $json | ConvertTo-Json -Depth 10

Write-Debug "New JSON:"
Write-Debug "---- BEGIN ----"
Write-Debug $jsonString
Write-Debug "---- END ----"

# Write the updated JSON string back to the appsettings.json file
$jsonString | Set-Content -Path $SettingsFile

if($IsLinux){
    #install APR library for linux
    sudo apt-get install apache2-dev libapr1-dev libaprutil1-dev
}

Write-Output "Entering '$PerfPath'"
Push-Location $PerfPath

try {
    mkdir build
    Push-Location build
    try {

        # Build the performance tests
        Write-Output "Building performance test"
        cmake ..
        cmake --build .

        Write-Output "Building service"
        dotnet build "$PerfPath" -c $Configuration /p:Platform=$Arch

        # When running the performance tests, set the data file name manually,
        # then unset once we're done
        Write-Output "Running performance test"

        ./runPerf.ps1 -c $Configuration -p $Arch 
		
        Get-ChildItem -Path $PerfPath -Filter "summary.json" -File -Recurse | ForEach-Object {
            $destinationPath = Join-Path -Path $PerfPath/build -ChildPath $_.Name
            Copy-Item -Path $_.FullName -Destination $destinationPath -Force -ErrorAction SilentlyContinue
            Write-Host "Copied $($_.Name) to $destinationPath"
        }
    }
    finally {

        # Output the results as it's useful for debugging.
        $files = Get-ChildItem -Filter "*.out" -File -Recurse
        foreach ($file in $files) {
            Write-Output "$($file.Name) :"
            Get-Content $file
        }

        Write-Output "Leaving build"
        Pop-Location

    }
}
finally {

    Write-Output "Leaving '$PerfPath'"
    Pop-Location

}