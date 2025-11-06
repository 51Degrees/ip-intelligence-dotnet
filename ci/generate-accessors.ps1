param (
    [Parameter(Mandatory)][string]$RepoName
)
$ErrorActionPreference = "Stop"
$PSNativeCommandUseErrorActionPreference = $true

./tools/ci/generate-accessors.ps1 @PSBoundParameters

Copy-Item "tools/CSharp/IIpIntelligenceData.cs" "ip-intelligence-dotnet/FiftyOne.IpIntelligence.Shared/Data/"
Copy-Item "tools/CSharp/IpIntelligenceDataBase.cs" "ip-intelligence-dotnet/FiftyOne.IpIntelligence.Shared/"
