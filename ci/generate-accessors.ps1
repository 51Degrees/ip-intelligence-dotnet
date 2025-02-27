param (
    [Parameter(Mandatory)][string]$RepoName,
    [string]$DataFile = "$PWD/$RepoName/51Degrees-EnterpriseV4.ipi"
)
$ErrorActionPreference = "Stop"

./tools/ci/generate-accessors.ps1 @PSBoundParameters

Copy-Item "tools/CSharp/IIpIntelligenceData.cs" "ip-intelligence-dotnet/FiftyOne.IpIntelligence.Data/Data/"
Copy-Item "tools/CSharp/IpIntelligenceDataBase.cs" "ip-intelligence-dotnet/FiftyOne.IpIntelligence.Data/"
