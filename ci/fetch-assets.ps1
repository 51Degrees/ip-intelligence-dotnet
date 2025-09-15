
param (
    [string]$RepoName,
    [Parameter(Mandatory=$true)]
    [string]$DeviceDetection,
    [string]$DeviceDetectionUrl
)

$ErrorActionPreference = "Stop"
$PSNativeCommandUseErrorActionPreference = $true

$CxxCiDir = Join-Path $pwd $RepoName "FiftyOne.IpIntelligence.Engine.OnPremise" "ip-intelligence-cxx"

# Use IP Intelligence naming internally
$IpIntelligence = $DeviceDetection
$IpIntelligenceUrl = $DeviceDetectionUrl

# Fetch the enterprise IPI data file for testing with
$DataFileName = "51Degrees-EnterpriseIpiV41.ipi"

# TODO: Use `fetch-hash-assets.ps1`
# ./steps/fetch-hash-assets.ps1 -RepoName $RepoName -LicenseKey $IpIntelligence -Url $IpIntelligenceUrl -DataType "IpIntelligenceV41" -ArchiveName $DataFileName
$ArchivedName = "51Degrees-EnterpriseIpiV41.ipi"
$ArchiveName = "$ArchivedName.gz"
Invoke-WebRequest -Uri $IpIntelligenceUrl -OutFile $RepoName/$ArchiveName
$ArchiveHash = (Get-FileHash -Algorithm MD5 -Path $RepoName/$ArchiveName).Hash
Write-Output "MD5 (fetched $ArchiveName) = $ArchiveHash"
Write-Output "Extracting $ArchiveName"
./steps/gunzip-file.ps1 $RepoName/$ArchiveName
Move-Item -Path $RepoName/$ArchivedName -Destination $RepoName/$DataFileName

$DataFileHash = (Get-FileHash -Algorithm MD5 -Path $RepoName/$DataFileName).Hash
Write-Output "MD5 (fetched $DataFileName) = $DataFileHash"

# Move the data file to the correct location
$DataFileSource = [IO.Path]::Combine($pwd, $RepoName, $DataFileName)
$DataFileDir = [IO.Path]::Combine($CxxCiDir, "ip-intelligence-data")
$DataFileDestination = [IO.Path]::Combine($DataFileDir, $DataFileName)
Move-Item $DataFileSource $DataFileDestination

# Get the evidence files for testing. These are in the device-detection-data submodule,
# But are not pulled by default.
Push-Location $DataFileDir
try {
    # Write-Output "Pulling evidence files"
    # git lfs pull

    foreach ($NextIpiFile in (Get-ChildItem "*.ipi" | ForEach-Object { $_.Name })) {
        $IpiFileHash = (Get-FileHash -Algorithm MD5 -Path $NextIpiFile).Hash
        Write-Output "MD5 ($NextIpiFile) = $IpiFileHash"
    }

    ./evidence-gen.ps1 -v4 10000 -v6 10000
    ./evidence-gen.ps1 -v4 10000 -v6 10000 -csv "evidence.csv"
}
finally {
    Pop-Location
}

exit $LASTEXITCODE
