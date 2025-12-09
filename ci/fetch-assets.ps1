param (
    [string]$DeviceDetection,
    [string]$DeviceDetectionUrl
)
$ErrorActionPreference = "Stop"

$ipIntelligenceData = "$PSScriptRoot/../FiftyOne.IpIntelligence.Engine.OnPremise/ip-intelligence-cxx/ip-intelligence-data"

# TODO: fix DeviceDetectionUrl containing IpIntelligenceUrl
./steps/fetch-assets.ps1 -DeviceDetection $DeviceDetection -IpIntelligenceUrl $DeviceDetectionUrl -Assets '51Degrees-EnterpriseIpiV41.ipi', '20000 User Agents.csv'
New-Item -ItemType SymbolicLink -Force -Target "$PWD/assets/51Degrees-EnterpriseIpiV41.ipi" -Path "$ipIntelligenceData/51Degrees-EnterpriseIpiV41.ipi"

Get-PSDrive -PSProvider FileSystem

Write-Host "Assets hashes:"
Get-FileHash -Algorithm MD5 -Path assets/*

Push-Location $ipIntelligenceData
try {
    ./evidence-gen.ps1 -v4 10000 -v6 10000
    ./evidence-gen.ps1 -v4 10000 -v6 10000 -csv -path "evidence.csv"
} finally {
    Pop-Location
}
