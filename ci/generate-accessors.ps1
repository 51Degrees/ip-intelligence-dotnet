param (
    [Parameter(Mandatory)][string]$RepoName,
    [string]$MetaDataPath = "$PWD/common-metadata",
    [string]$DataType = "IpIntelligence"
)
$ErrorActionPreference = "Stop"

./tools/ci/generate-accessors.ps1 -RepoName:$RepoName -MetaDataPath:$MetaDataPath -DataType:$DataType

Copy-Item "tools/CSharp/IIpIntelligenceData.cs" "ip-intelligence-dotnet/FiftyOne.IpIntelligence.Shared/Data/"
Copy-Item "tools/CSharp/IpIntelligenceDataBase.cs" "ip-intelligence-dotnet/FiftyOne.IpIntelligence.Shared/"
