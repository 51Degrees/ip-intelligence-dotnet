param(
    [Parameter(Mandatory)][string]$RepoName,
    [string]$OrgName,
    [string]$Name = "Release_x64",
    [string]$Configuration = "Release",
    [string]$Arch = "x64"
)

Write-Host "Running console performance tests..."
& $PSScriptRoot/run-performance-tests-console.ps1 -Debug -RepoName:$RepoName -OrgName:$OrgName -Name:$Name -Configuration:$Configuration -Arch:$Arch

if ($env:SKIP_WEB_PERFORMANCE -eq 'true') {
    Write-Host "Skipping web performance tests (SKIP_WEB_PERFORMANCE)."
} else {
    Write-Host "Running web performance tests..."
    & $RepoName/ci/run-performance-tests-web.ps1 -Name:$Name -Configuration:$Configuration -Arch:$Arch
}
