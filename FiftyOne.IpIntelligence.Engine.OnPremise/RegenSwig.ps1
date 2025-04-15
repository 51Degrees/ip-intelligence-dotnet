[CmdletBinding()]
param(
    [string]$swigExec = "swig"
)

Write-Debug "Removing old files..."

Remove-Item Interop/Swig/*.cs
Remove-Item IpIntelligenceEngineSwig_csharp.cpp

Write-Debug "Invoking SWIG executable..."

& $swigExec `
    -c++ -csharp `
    -namespace FiftyOne.IpIntelligence.Engine.OnPremise.Interop `
    -module IpIntelligenceEngineModule `
    -dllimport FiftyOne.IpIntelligence.Engine.OnPremise.Native.dll `
    -outdir Interop/Swig `
    -o IpIntelligenceEngineSwig_csharp.cpp `
    ipi_csharp.i

Write-Debug "LASTEXITCODE = $LASTEXITCODE"

exit $LASTEXITCODE
