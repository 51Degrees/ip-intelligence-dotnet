[CmdletBinding()]
param(
    [Parameter(Mandatory=$true)]
    [string]$RepoName,
    [string]$OrgName,
    [string]$ProjectDir = ".",
    [string]$Name = "Release_x64",
    [string]$Configuration = "Release",
    [string]$Arch = "x64"
)

Write-Output "Running console performance tests..."
& ./$RepoName/ci/run-performance-tests-console.ps1 `
    -Debug `
    -RepoName $RepoName `
    -OrgName $OrgName `
    -Name $Name `
    -Configuration $Configuration `
    -Arch $Arch
if ($LASTEXITCODE -ne 0) {
    Write-Warning "LASTEXITCODE = $LASTEXITCODE"
    exit $LASTEXITCODE
}

Write-Output "Running web performance tests..."
& ./$RepoName/ci/run-performance-tests-web.ps1 `
    -Debug `
    -RepoName $RepoName `
    -ProjectDir $ProjectDir `
    -Name $Name `
    -Configuration $Configuration `
    -Arch $Arch
if ($LASTEXITCODE -ne 0) {
    Write-Warning "LASTEXITCODE = $LASTEXITCODE"
}

exit $LASTEXITCODE
