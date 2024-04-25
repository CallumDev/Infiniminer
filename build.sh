#!/bin/bash
./extern/Librelancer/build.sh GenerateVersion
mkdir -p bin/natives/x64

mkdir -p obj/linux
mkdir -p obj/windows

#Compile Linux Natives
cd obj/linux
cmake -DCMAKE_BUILD_TYPE=Release ../..
make -j$(nproc)
cp binaries/*.so ../../bin/natives/
cd ../..

#Compile Windows Natives
cd obj/windows
cmake -DCMAKE_BUILD_TYPE=Release -DCMAKE_TOOLCHAIN_FILE=../../extern/Librelancer/scripts/mingw-w64-x86_64.cmake ../..
make -j$(nproc)
cp binaries/*.dll ../../bin/natives/x64/
cd ../..
cp extern/Librelancer/deps/x64/SDL2.dll bin/natives/x64/
cp extern/Librelancer/deps/x64/openal32.dll bin/natives/x64/

PUBLISH_LINUX="dotnet publish -r linux-x64 --sc -c Release"
PUBLISH_WIN64="dotnet publish -r win-x64 --sc -c Release"
PUBLISH_TOOLS="dotnet run --project PublishTools/PublishTools.csproj"

# Publish Client
$PUBLISH_LINUX -o bin/infiniminer-linux/lib/ Infiniminer/Infiniminer.csproj
$PUBLISH_TOOLS bin/infiniminer-linux/lib/Infiniminer linux
mv bin/infiniminer-linux/lib/Infiniminer bin/infiniminer-linux/Infiniminer
rm -r bin/infiniminer-linux/Content
mv bin/infiniminer-linux/lib/Content bin/infiniminer-linux/
mv bin/infiniminer-linux/lib/*.txt bin/infiniminer-linux/

$PUBLISH_WIN64 -o bin/infiniminer-win64/lib/ Infiniminer/Infiniminer.csproj
$PUBLISH_TOOLS bin/infiniminer-win64/lib/Infiniminer.exe winexe
mv bin/infiniminer-win64/lib/Infiniminer.exe  bin/infiniminer-win64/Infiniminer.exe
rm -r bin/infiniminer-win64/Content
mv bin/infiniminer-win64/lib/Content bin/infiniminer-win64/
mv bin/infiniminer-win64/lib/*.txt bin/infiniminer-win64/

#Publish Server
$PUBLISH_LINUX -o bin/infiniminerserver-linux/lib/ InfiniminerServer/InfiniminerServer.csproj
$PUBLISH_TOOLS bin/infiniminerserver-linux/lib/InfiniminerServer linux
mv bin/infiniminerserver-linux/lib/InfiniminerServer bin/infiniminerserver-linux/InfiniminerServer
mv bin/infiniminerserver-linux/lib/*.txt bin/infiniminerserver-linux/

$PUBLISH_WIN64 -o bin/infiniminerserver-win64/lib/ InfiniminerServer/InfiniminerServer.csproj
$PUBLISH_TOOLS bin/infiniminerserver-win64/lib/InfiniminerServer.exe winconsole
mv bin/infiniminerserver-win64/lib/InfiniminerServer.exe bin/infiniminerserver-win64/InfiniminerServer.exe
mv bin/infiniminerserver-win64/lib/*.txt bin/infiniminerserver-win64/
