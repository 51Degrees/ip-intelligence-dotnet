@for %%X in (swig.exe) do (set SWIG_EXE=%%~$PATH:X)
@if defined SWIG_EXE (
@echo SWIG auto generated code being rebuilt.
swig -c++ -csharp -namespace FiftyOne.IpIntelligence.Engine.OnPremise.Interop -dllimport FiftyOne.IpIntelligence.Engine.OnPremise.Native.dll -outdir ../FiftyOne.IpIntelligence.Engine.OnPremise/Interop/Swig -o ../IpIntelligenceEngineSwig_csharp.cpp ipi_csharp.i
) else (
@echo SWIG not found. SWIG auto generated code will not be rebuilt.
)