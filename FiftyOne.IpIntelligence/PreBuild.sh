#!/bin/bash


SCRIPTPATH="$( cd "$(dirname "$0")" ; pwd -P )"

SRCOUT=$SCRIPTPATH/FiftyOne.IpIntelligence.Engine.OnPremise/Interop/Swig
RES=$SCRIPTPATH/dlls

SRCMAIN=$SCRIPTPATH/ip-intelligence-cxx/src
SRCCM=$SRCMAIN/common-cxx
TH="-D FIFTYONEDEGREES_NO_THREADING"
GCCARGS="-c -std=c11 -fPIC -pthread -O2"
GXXARGS="-c -std=c++11 -fPIC -pthread -O2"
LDARGS="-shared -O2 -pthread -std=c++11"
unameOut="$(uname -s)"
case "${unameOut}" in
    Linux*)
    OS=linux
    LDARGS="$LDARGS -static-libgcc -static-libstdc++"
    ;;
    Darwin*)
    OS=mac
    ;;
#    CYGWIN*)    OS=Cygwin;;
#    MINGW*)     OS=MinGw;;
    *)          OS="UNKNOWN:${unameOut}"
esac

if [ "$OS" = "UNKNOWN:${unameOut}" ]; then
  { echo >&2 "Operating system is UNKNOWN:${unameOut}. Aborting."; exit 1; }
fi

if command -v swig >/dev/null 2>&1; then
    swig -c++ -csharp -namespace FiftyOne.IpIntelligence.Engine.OnPremise.Interop -dllimport FiftyOne.IpIntelligence.Engine.OnPremise.Native.dll -outdir $SRCOUT -o $SCRIPTPATH/IpIntelligenceEngineSwig_csharp.cpp $SCRIPTPATH/FiftyOne.IpIntelligence.Engine.OnPremise.Native/ipi_csharp.i
else
    { echo >&2 "Swig is required to generate wrapper but it's not installed."; }
fi

rm -r obj/
mkdir obj

echo "Building IP Intelligence native library."
for ARCH in "x86" "x64"
do
    echo $ARCH
    if [ "$ARCH" = "x86" ]
    then
        M=-m32
    else
        M=-m64
    fi

    # SWIG
    g++ $M $GXXARGS $SCRIPTPATH/IpIntelligenceEngineSwig_csharp.cpp -o obj/IpIntelligenceEngineSwig_csharp.o

    # IP Intelligence
	g++ $M $GXXARGS $SRCMAIN/ComponentMetaDataBuilderIpi.cpp -o obj/ComponentMetaDataBuilderIpi.o
    g++ $M $GXXARGS $SRCMAIN/ComponentMetaDataCollectionIpi.cpp -o obj/ComponentMetaDataCollectionIpi.o
    g++ $M $GXXARGS $SRCMAIN/ConfigIpi.cpp -o obj/ConfigIpi.o
    g++ $M $GXXARGS $SRCMAIN/EngineIpi.cpp -o obj/EngineIpi.o
	g++ $M $GXXARGS $SRCMAIN/EvidenceIpi.cpp -o obj/EvidenceIpi.o
	g++ $M $GXXARGS $SRCMAIN/IpAddress.cpp -o obj/IpAddress.o
    g++ $M $GXXARGS $SRCMAIN/MetaDataIpi.cpp -o obj/MetaDataIpi.o
	g++ $M $GXXARGS $SRCMAIN/ProfileMetaDataBuilderIpi.cpp -o obj/ProfileMetaDataBuilderIpi.o
	g++ $M $GXXARGS $SRCMAIN/ProfileMetaDataCollectionIpi.cpp -o obj/ProfileMetaDataCollectionIpi.o
	g++ $M $GXXARGS $SRCMAIN/PropertyMetaDataBuilderIpi.cpp -o obj/PropertyMetaDataBuilderIpi.o
    g++ $M $GXXARGS $SRCMAIN/PropertyMetaDataCollectionForComponentIpi.cpp -o obj/PropertyMetaDataForComponentIpi.o
	g++ $M $GXXARGS $SRCMAIN/PropertyMetaDataCollectionForPropertyIpi.cpp -o obj/PropertyMetaDataCollectionForPropertyIpi.o
    g++ $M $GXXARGS $SRCMAIN/PropertyMetaDataCollectionIpi.cpp -o obj/PropertyMetaDataCollectionIpi.o
    g++ $M $GXXARGS $SRCMAIN/ResultsIpi.cpp -o obj/ResultsIpi.o
	g++ $M $GXXARGS $SRCMAIN/ValueMetaDataBuilderIpi.cpp -o obj/ValueMetaDataBuilderIpi.o
	g++ $M $GXXARGS $SRCMAIN/ValueMetaDataCollectionBaseIpi.cpp -o obj/ValueMetaDataCollectionBaseIpi.o
	g++ $M $GXXARGS $SRCMAIN/ValueMetaDataCollectionForProfileIpi.cpp -o obj/ValueMetaDataCollectionForProfileIpi.o
	g++ $M $GXXARGS $SRCMAIN/ValueMetaDataCollectionForPropertyIpi.cpp -o obj/ValueMetaDataCollectionForPropertyIpi.o
	g++ $M $GXXARGS $SRCMAIN/ValueMetaDataCollectionIpi.cpp -o obj/ValueMetaDataCollectionIpi.o
    gcc $M $GCCARGS $SRCMAIN/ipi.c -o obj/ipi.o

    # Common
    g++ $M $GXXARGS $SRCCM/CollectionConfig.cpp -o obj/CollectionConfig.o
    g++ $M $GXXARGS $SRCCM/ComponentMetaData.cpp -o obj/ComponentMetaData.o
    g++ $M $GXXARGS $SRCCM/ConfigBase.cpp -o obj/ConfigBase.o
    g++ $M $GXXARGS $SRCCM/Date.cpp -o obj/Date.o
    g++ $M $GXXARGS $SRCCM/EngineBase.cpp -o obj/EngineBase.o
    g++ $M $GXXARGS $SRCCM/EvidenceBase.cpp -o obj/EvidenceBase.o
    g++ $M $GXXARGS $SRCCM/Exceptions.cpp -o obj/Exceptions.o
    g++ $M $GXXARGS $SRCCM/MetaData.cpp -o obj/MetaData.o
    g++ $M $GXXARGS $SRCCM/ProfileMetaData.cpp -o obj/ProfileMetaData.o
    g++ $M $GXXARGS $SRCCM/PropertyMetaData.cpp -o obj/PropertyMetaData.o
    g++ $M $GXXARGS $SRCCM/RequiredPropertiesConfig.cpp -o obj/RequiredPropertiesConfig.o
    g++ $M $GXXARGS $SRCCM/ResultsBase.cpp -o obj/ResultsBase.o
    g++ $M $GXXARGS $SRCCM/ValueMetaData.cpp -o obj/ValueMetaData.o
    gcc $M $GCCARGS $SRCCM/cache.c -o obj/cache.o
    gcc $M $GCCARGS $SRCCM/collection.c -o obj/collection.o
    gcc $M $GCCARGS $SRCCM/component.c -o obj/component.o
	gcc $M $GCCARGS $SRCCM/coordinate.c -o obj/coordinate.o
    gcc $M $GCCARGS $SRCCM/data.c -o obj/data.o
    gcc $M $GCCARGS $SRCCM/dataset.c -o obj/dataset.o
    gcc $M $GCCARGS $SRCCM/evidence.c -o obj/evidence.o
    gcc $M $GCCARGS $SRCCM/exceptionsc.c -o obj/exceptionsc.o
    gcc $M $GCCARGS $SRCCM/file.c -o obj/file.o
	gcc $M $GCCARGS $SRCCM/float.c -o obj/float.o
    gcc $M $GCCARGS $SRCCM/headers.c -o obj/headers.o
    gcc $M $GCCARGS $SRCCM/list.c -o obj/list.o
    gcc $M $GCCARGS $SRCCM/memory.c -o obj/memory.o
    gcc $M $GCCARGS $SRCCM/overrides.c -o obj/overrides.o
    gcc $M $GCCARGS $SRCCM/pool.c -o obj/pool.o
    gcc $M $GCCARGS $SRCCM/profile.c -o obj/profile.o
    gcc $M $GCCARGS $SRCCM/properties.c -o obj/properties.o
    gcc $M $GCCARGS $SRCCM/property.c -o obj/property.o
    gcc $M $GCCARGS $SRCCM/resource.c -o obj/resource.o
    gcc $M $GCCARGS $SRCCM/results.c -o obj/results.o
    gcc $M $GCCARGS $SRCCM/status.c -o obj/status.o
    gcc $M $GCCARGS $SRCCM/string.c -o obj/string.o
    gcc $M $GCCARGS $SRCCM/threading.c -o obj/threading.o
    gcc $M $GCCARGS $SRCCM/tree.c -o obj/tree.o
    gcc $M $GCCARGS $SRCCM/value.c -o obj/value.o

    mkdir -p $RES/$OS/$ARCH

    # Shared Library
    g++ $M $LDARGS obj/*.o -o $RES/$OS/$ARCH/libFiftyOne.IpIntelligence.Engine.OnPremise.Native.dll
done
