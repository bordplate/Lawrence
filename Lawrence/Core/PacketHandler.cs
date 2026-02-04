
using System;
using System.Reflection;
using Lawrence.Core;

public class PacketContext {
    public required Client Client { get; init; }
    public required MPPacketHeader Header { get; init; }
    public required byte[] Body { get; init; }
    public required MPPacketType PacketType { get; init; }
    public required bool RequiresHandshake { get; init; }
    public bool HaveRepliedToRpc = true;
    public bool RequiresDirectoryMode = false;
    
    public T Get<T>() where T: struct, MPPacket {
        if (Packet.BytesToStruct<T>(Body, Client.Endianness) is not { } payload) {
            throw new NetworkParsingException($"Failed to parse packet {PacketType} to {typeof(T)}");
        }

        return payload;
    }

    public uint GetPacketSize() {
        uint packetSize = 0;
        if (Header.Size > 0 && Header.Size < 1024 * 8) {
            packetSize = Header.Size;
        }

        return packetSize;
    }

    public IClientHandler? GetHandler() {
        return Client.ClientHandler;
    }
    
    public void RPCRespond<T>(T[] packets) where T : struct, MPPacket {
        var packet = new Packet(MPPacketType.MP_PACKET_ACK, Header.RequiresAck, Header.AckCycle);

        foreach (var p in packets) {
            packet.AddBodyPart(p);
        }
        
        Client.SendPacket(packet);

        HaveRepliedToRpc = true;
    }
}

[AttributeUsage(AttributeTargets.Method)]
public class PacketHandler(MPPacketType packetType) : Attribute {
    public readonly MPPacketType PacketType = packetType;
    private Action<PacketContext>? _handler = null;
    
    public bool RequiresHandshake = true;
    public bool RequiresDirectoryMode = false;

    public void SetHandler(Action<PacketContext>? handler) {
        _handler = handler;
    }

    public void SetHandler(MethodInfo handlerMethodInfo) {
        _handler = (Action<PacketContext>)Delegate.CreateDelegate(typeof(Action<PacketContext>), handlerMethodInfo);
    }

    public void Handle(Client client, MPPacketHeader header, byte[] body) {
        if (_handler is null) {
            throw new Exception($"PacketHandler {PacketType} has not been implemented");
        }

        PacketContext context = new() {
            PacketType = PacketType,
            Client = client,
            Header = header,
            Body = body,
            RequiresHandshake = RequiresHandshake,
            RequiresDirectoryMode = RequiresDirectoryMode
        };

        var pipeline = client.Server.Pipeline.Get(_handler);
        pipeline(context);
    }
}
