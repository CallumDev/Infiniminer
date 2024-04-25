using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using LibreLancer;
using LiteNetLib;
using LiteNetLib.Utils;

namespace Infiniminer;

public class NetClient
{
    public NetManager Manager;

    record NetEvent(NetEventType Type, IPEndPoint Remote, NetDataReader Data);
    ConcurrentQueue<NetEvent> evqueue = new ConcurrentQueue<NetEvent>();

    private NetPeer peer;

    NetDataReader PacketString(string str)
    {
        var w = new NetDataWriter();
        w.Put(str);
        return new NetDataReader(w);
    }

    public ConnectionState ConnectionState => peer?.ConnectionState ?? ConnectionState.Disconnected;

    public void Connect(IPEndPoint endPoint, NetDataWriter writer)
    {
        peer = Manager.Connect(endPoint, writer);
    }

    public void Disconnect(string reason)
    {
        peer?.DisconnectReason(reason);
    }

    public void DiscoverLocalServers(int port)
    {
        var dw = new NetDataWriter();
        for (int i = 0; i < Defines.AppSignature.Length; i++)
            dw.Put(Defines.AppSignature[i]);
        Manager.SendBroadcast(dw, port);
    }

    IPEndPoint GetEndpoint(NetPeer peer) => new IPEndPoint(peer.Address, peer.Port);

    public void Send(NetDataWriter writer, DeliveryMethod deliveryMethod) => peer?.Send(writer, deliveryMethod);

    
    public NetClient()
    {
        LiteNetLib.NetDebug.Logger = new Logger();
        EventBasedNetListener events = new EventBasedNetListener();
        Manager = new NetManager(events)
        {
            IPv6Enabled = true,
            ChannelsCount = 4,
            UnconnectedMessagesEnabled = true
        };
        events.NetworkReceiveUnconnectedEvent += (point, reader, type) =>
        {
            if (type == UnconnectedMessageType.BasicMessage)
            {
                Span<byte> sig = stackalloc byte[Defines.AppSignature.Length];
                if (reader.TryGetSpan(sig) && sig.SequenceEqual(Defines.AppSignature)) {
                    evqueue.Enqueue(new NetEvent(NetEventType.ServerDiscovered, point, reader));
                }
            }
        };
        events.NetworkReceiveEvent += (peer, reader, channel, method) =>
        {
            evqueue.Enqueue(new NetEvent(NetEventType.Packet, GetEndpoint(peer), CopyPacket(reader)));
        };
        events.PeerConnectedEvent += peer =>
        {
            evqueue.Enqueue(new NetEvent(NetEventType.Approved, GetEndpoint(peer), null));
        };
        events.PeerDisconnectedEvent += (peer, info) =>
        {
            if (info.AdditionalData.TryGetByte(out var res)) {
                if (res == 0xCC)
                {
                    evqueue.Enqueue(new NetEvent(NetEventType.Rejected, GetEndpoint(peer), CopyPacket(info.AdditionalData)));
                }
                else if (res == 0xBA)
                {
                    evqueue.Enqueue(new NetEvent(NetEventType.Disconnected, GetEndpoint(peer), CopyPacket(info.AdditionalData)));
                }
                else
                {
                    evqueue.Enqueue(new NetEvent(NetEventType.Disconnected, GetEndpoint(peer), PacketString(info.Reason.ToString())));
                }
            }
            else
            {
                evqueue.Enqueue(new NetEvent(NetEventType.Disconnected, GetEndpoint(peer), PacketString(info.Reason.ToString())));
            }
        };
        Manager.Start();
    }

    class Logger : INetLogger
    {
        public void WriteNet(NetLogLevel level, string str, params object[] args)
        {
            FLLog.Debug("Net", $"{level}: {string.Format(str, args)}");
        }
    }

    NetDataReader CopyPacket(NetDataReader src)
    {
        var dr = new NetDataReader(src.RawData.ToArray(), src.UserDataOffset, src.RawDataSize);
        dr.SetPosition(src.Position);
        return dr;
    }

    public bool ReadMessage(out IPEndPoint remote, out NetDataReader data, out NetEventType type)
    {
        Manager.PollEvents();
        remote = null;
        data = null;
        type = NetEventType.Invalid;
        if(evqueue.TryDequeue(out var result))
        {
            remote = result.Remote;
            data = result.Data;
            type = result.Type;
            return true;
        }
        return false;
    }
}

public enum NetEventType
{
    Disconnected,
    Approved,
    Rejected,
    Packet,
    ServerDiscovered,
    Invalid
}

