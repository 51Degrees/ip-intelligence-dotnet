
param (
    [string]$RepoName,
    [Parameter(Mandatory=$true)]
    [string]$IpIntelligence,
    [string]$IpIntelligenceUrl
)

$CxxCiDir = Join-Path $RepoName "FiftyOne.IpIntelligence.Engine.OnPremise" "ip-intelligence-cxx"
$CxxCiScript = Join-Path $pwd $CxxCiDir "ci" "fetch-assets.ps1"

& $CxxCiScript `
    -RepoName $CxxCiDir `
    -IpIntelligence $IpIntelligence `
    -IpIntelligenceUrl $IpIntelligenceUrl

exit $LASTEXITCODE
