#!/bin/bash

if command -v swig >/dev/null 2>&1; then
    { echo >&2 "Generating Swig wrapper for Hash."; }
    swig -c++ -csharp -namespace FiftyOne.IpIntelligence.Engine.OnPremise.Interop -dllimport FiftyOne.IpIntelligence.Engine.OnPremise.Native.dll -outdir ../FiftyOne.IpIntelligence.Engine.OnPremise/Interop/Swig -o ../IpIntelligenceEngineSwig_csharp.cpp ipi_csharp.i
else
    { echo >&2 "Swig is required to generate wrapper but it's not installed."; }
fi
