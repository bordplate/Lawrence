using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace Lawrence.Core;


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

        Action<PacketContext> current = ctx => {
            _handler(ctx);
        };

        var middlewares = client.Server.Middlewares;
        for (var i = middlewares.Count - 1; i >= 0; i--) {
            var next = current;
            var middleware = middlewares[i];
            current = ctx => middleware.Execute(ctx, next);
        }

        current(context);
    }
}

public class PacketRouter(Client client) {
    private static Dictionary<MPPacketType, PacketHandler> PacketHandlers = new ();

    public void RoutePacket(MPPacketHeader header, byte[] body) {
        PacketHandler? handler;
        PacketHandlers.TryGetValue(header.PacketType, out handler);

        if (handler is null) {
            Logger.Error(
                $"(Player {client.ID}) sent unknown (possibly malformed) packet {header.PacketType} with size: {header.Size}.");
            return;
        }
        
        handler.Handle(client, header, body);
    }

    public static void ResolveHandlers() {
        var assembly = Assembly.GetExecutingAssembly();
        var methods = assembly.GetTypes().SelectMany(t =>
                t.GetMethods(BindingFlags.Static | BindingFlags.Public)
            )
            .Where(m => m.GetCustomAttribute<PacketHandler>() is not null)
            .ToList();

        foreach (var method in methods) {
            if (method.GetCustomAttribute<PacketHandler>() is not { } attribute) {
                continue;
            }
            
            attribute.SetHandler(method);
            PacketHandlers[attribute.PacketType] = attribute;
        }
    }
}