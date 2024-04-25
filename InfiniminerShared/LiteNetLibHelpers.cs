using System.Numerics;
using LiteNetLib;
using LiteNetLib.Utils;

namespace Infiniminer;

public static class LiteNetLibHelpers
{
    public static void Reject(this ConnectionRequest request, string reason)
    {
        var dw = new NetDataWriter();
        dw.Put((byte)0xBA);
        dw.Put(reason);
        request.Reject(dw);
    }

    public static void Put(this NetDataWriter writer, Vector3 v)
    {
        writer.Put(v.X);
        writer.Put(v.Y);
        writer.Put(v.Z);
    }

    public static Vector3 GetVector3(this NetDataReader reader)
    {
        return new Vector3(reader.GetFloat(), reader.GetFloat(), reader.GetFloat());
    }
}