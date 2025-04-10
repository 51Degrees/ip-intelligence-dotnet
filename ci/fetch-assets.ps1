
param (
    [string]$RepoName,
    [Parameter(Mandatory=$true)]
    [string]$DeviceDetection,
    [string]$DeviceDetectionUrl
)
$ErrorActionPreference = "Stop"
$PSNativeCommandUseErrorActionPreference = $true

# File download doesn't work yet
exit 0

# Fetch the TAC data file for testing with
$DataFileName = "TAC-IpIntelligenceV41.ipi"

./steps/fetch-hash-assets.ps1 -RepoName $RepoName -LicenseKey $DeviceDetection -Url $DeviceDetectionUrl -DataType "IpIntelligenceV41" -ArchiveName $DataFileName
# $ArchivedName = "51Degrees-EnterpriseIpiV41.ipi 1"
# $ArchiveName = "$ArchivedName.gz"
# Invoke-WebRequest -Uri $DeviceDetectionUrl -OutFile $RepoName/$ArchiveName
# $ArchiveHash = (Get-FileHash -Algorithm MD5 -Path $RepoName/$ArchiveName).Hash
# Write-Output "MD5 (fetched $ArchiveName) = $ArchiveHash"
# Write-Output "Extracting $ArchiveName"
# ./steps/gunzip-file.ps1 $RepoName/$ArchiveName
# Move-Item -Path $RepoName/$ArchivedName -Destination $RepoName/$DataFileName

$DataFileHash = (Get-FileHash -Algorithm MD5 -Path $RepoName/$DataFileName).Hash
Write-Output "MD5 (fetched $DataFileName) = $DataFileHash"


# Move the data file to the correct location
$DataFileSource = [IO.Path]::Combine($pwd, $RepoName, $DataFileName)
$DataFileDir = [IO.Path]::Combine($pwd, $RepoName, "FiftyOne.IpIntelligence", "ip-intelligence-cxx", "ip-intelligence-data")
$DataFileDestination = [IO.Path]::Combine($DataFileDir, $DataFileName)
Move-Item $DataFileSource $DataFileDestination

# Get the evidence files for testing. These are in the device-detection-data submodule,
# But are not pulled by default.
Push-Location $DataFileDir
try {
    Write-Output "Pulling evidence files"
    # git lfs pull

    # Use Enterprise as Lite
    Copy-Item $DataFileName 51Degrees-LiteV41.ipi

    foreach ($NextIpiFile in (Get-ChildItem "*.ipi" | ForEach-Object { $_.Name })) {
        $IpiFileHash = (Get-FileHash -Algorithm MD5 -Path $NextIpiFile).Hash
        Write-Output "MD5 ($NextIpiFile) = $IpiFileHash"
    }

    ./evidence-gen.ps1 -v4 10000 -v6 10000
}
finally {
    Pop-Location
}