// See https://aka.ms/new-console-template for more information

using System.Reflection.PortableExecutable;
using System.Text;

const int E_LFANEW = 0x3C;
const int SUBSYSTEM_OFFSET = 0x5C;

var appHostPath = args[0];
bool winexe = args[1] == "winexe";
bool isWin = args[1] == "winexe" || args[1] == "winconsole";

Console.WriteLine($"Patching {appHostPath} as {args[1]}");

static int FindBytes(byte[] bytes, byte[] pattern) {
    int idx = 0;
    var first = pattern[0];
    while (idx < bytes.Length) {
        idx = Array.IndexOf(bytes, first, idx);
        if (idx < 0) break; //Not Found
        if (BytesEqual(bytes, idx, pattern))
            return idx;
        idx++;
    }
    return -1;
}

static bool BytesEqual(byte[] bytes, int index, byte[] pattern) {
    if (index + pattern.Length > bytes.Length)
        return false;
    for (int i = 0; i < pattern.Length; i++) {
        if (bytes[index + i] != pattern[i])
            return false;
    }
    return true;
}
var origName = Path.GetFileNameWithoutExtension(appHostPath) + ".dll";
var origPathBytes = Encoding.UTF8.GetBytes(origName + "\0");
var libDir = isWin ? "lib\\" : "lib/";
var newPath = libDir + origName;
var newPathBytes = Encoding.UTF8.GetBytes(newPath + "\0");
var apphostExe = File.ReadAllBytes(appHostPath);
int offset = FindBytes(apphostExe, origPathBytes);
if(offset < 0) {
    throw new Exception("Could not patch apphost " + appHostPath);
}
for(int i = 0; i < newPathBytes.Length; i++)
    apphostExe[offset + i] = newPathBytes[i];
if (winexe)
{
    var peHeaderLocation = BitConverter.ToInt32(apphostExe, E_LFANEW);
    var subsystemLocation = peHeaderLocation + SUBSYSTEM_OFFSET;
    Console.WriteLine($"Patching subsystem for {appHostPath}");
    var winexeBytes = BitConverter.GetBytes((ushort) Subsystem.WindowsGui);
    apphostExe[subsystemLocation] = winexeBytes[0];
    apphostExe[subsystemLocation + 1] = winexeBytes[1];
}
File.WriteAllBytes(appHostPath, apphostExe);