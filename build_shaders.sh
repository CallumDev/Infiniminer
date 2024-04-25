#!/bin/bash
mkdir extern/Librelancer/shaders/natives/bin
cd extern/Librelancer/shaders/natives/bin
cmake -DCMAKE_BUILD_TYPE=Release ..
make -j$(nproc)
cd ../../../../..
dotnet run --project extern/Librelancer/shaders/ShaderProcessor/ShaderProcessor.csproj -- \
-b -g GLExtensions.Features430 -t Effect -c Effect.Compile -x Effect.Log -n Infiniminer.Effects -o ./InfiniminerShared/Effects/ ./Shaders/*.glsl
