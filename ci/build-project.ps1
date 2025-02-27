param(
    [Parameter(Mandatory=$true)]
    [string]$RepoName = "ip-intelligence-dotnet",
    [string]$ProjectDir = ".",
    [string]$Name = "Release_x64",
    [string]$Configuration = "Release",
    [string]$Arch = "x64",
    [string]$BuildMethod = "msbuild"
)
$ErrorActionPreference = "Stop"
$PSNativeCommandUseErrorActionPreference = $true

if ($BuildMethod -eq "dotnet"){

    if ($IsWindows) {
        ./dotnet/build-project-core.ps1 -RepoName $RepoName -ProjectDir $ProjectDir -Name $Name -Configuration $Configuration -Arch $Arch
    }
    else {
        # On non-Windows, use the solution filter to remove the Framework projects.
        ./dotnet/build-project-core.ps1 -RepoName $RepoName -ProjectDir "./FiftyOne.IpIntelligence.Core.slnf" -Name $Name -Configuration $Configuration -Arch $Arch
    }

}
else{
    ./dotnet/build-project-framework.ps1 -RepoName $RepoName -ProjectDir $ProjectDir -Name $Name -Configuration $Configuration -Arch $Arch
}

exit $LASTEXITCODE