$ErrorActionPreference = "Stop"
$PSNativeCommandUseErrorActionPreference = $true

./tools/ci/generate-dd-accessors.ps1

$ToolsPath = [IO.Path]::Combine($pwd, "tools")
$DdPath = [IO.Path]::Combine($pwd, "ip-intelligence-dotnet")

Copy-Item "$ToolsPath/CSharp/IIpIntelligenceData.cs" "$DdPath/FiftyOne.IpIntelligence.Data/Data/"
Copy-Item "$ToolsPath/CSharp/IpIntelligenceDataBase.cs" "$DdPath/FiftyOne.IpIntelligence.Data/"