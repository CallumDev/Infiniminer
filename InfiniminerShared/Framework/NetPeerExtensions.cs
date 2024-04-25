using System;
using LiteNetLib;
using LiteNetLib.Utils;

namespace Infiniminer;

public static class NetPeerExtensions
{
    public static void DisconnectReason(this NetPeer peer, string reason)
    {
        var writer = new NetDataWriter();
        writer.Put((byte)0xCC);
        writer.Put(reason);
        peer.Disconnect(writer);
    }
    
    public static bool TryGetSpan(this NetDataReader reader, Span<byte> dest)
    {
        var pos = reader.Position;
        for (int i = 0; i < dest.Length; i++)
        {
            if (!reader.TryGetByte(out dest[i]))
            {
                reader.SetPosition(pos);
                return false;
            }
        }
        return true;
    }
}