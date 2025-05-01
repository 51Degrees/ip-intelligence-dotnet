
param (
    [string]$RepoName,
    [Parameter(Mandatory=$true)]
    [string]$DeviceDetection,
    [string]$DeviceDetectionUrl
)

$CxxCiDir = Join-Path $RepoName "FiftyOne.IpIntelligence.Engine.OnPremise" "ip-intelligence-cxx" "ci"
$CxxCiScript = Join-Path $pwd $CxxCiDir "fetch-assets.ps1"

& $CxxCiScript `
    -RepoName $CxxCiDir `
    -DeviceDetection $DeviceDetection `
    -DeviceDetectionUrl $DeviceDetectionUrl

exit $LASTEXITCODE
