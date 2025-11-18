[CmdletBinding()]
param (
	$c,
	$d,
	$p,
	[switch]$NoScriptRoot
)

if ($c -eq $null) {
    Write-Host "Config was not provided, defaulting to Debug."
	$c="Debug"
}
if ($d -eq $null) {
	Write-Host "Dotnet path was not provided, defaulting to system dotnet."
	$d="dotnet"
}
if ($p -eq $null) {
	Write-Host "Platform was not provided, defaulting to x64."
	$p="x64"
}

if ($c -eq $null -or $c -eq $null) {
	Write-Host "Available options are"
    Write-Host "    -c : configuration e.g. Debug or Release"
	Write-Host "    -p : platform e.g. x86, x64 or AnyCPU"
    Write-Host "    -d : path to dotnet executable"
}

Write-Host "Configuration         = $c"
Write-Host "Platform              = $p"
Write-Host "DotNet                = $d"

$scriptRoot = $NoScriptRoot ? (Get-Location) : (Split-Path -Parent -Path $MyInvocation.MyCommand.Definition)

# Constants
$PASSES=20000
$SERVICEHOST="localhost:5000"
$CAL="calibrate"
$PRO="process"
$PERF="$scriptRoot/ApacheBench-prefix/src/ApacheBench-build/bin/runPerf.ps1"

Write-Debug "Looking for all runPerf.ps1"
Get-ChildItem -Recurse -File -Filter "runPerf.ps1"
Write-Debug "Checking executable"
Get-ChildItem $PERF
Write-Debug "Looking for all performance_tests.dll"
Get-ChildItem -Recurse -File -Filter "performance_tests.dll"
Write-Debug "Looking for specific performance_tests.dll"
$TargetDLL = "$pwd/../bin/$p/$c/net8.0/performance_tests.dll"
Get-ChildItem $TargetDLL

Write-Debug "[EXEC] >>> $PERF -n $PASSES -s '$d $TargetDLL' -c $CAL -p $PRO -h $SERVICEHOST"
Invoke-Expression "$PERF -n $PASSES -s '$d $TargetDLL' -c $CAL -p $PRO -h $SERVICEHOST"
