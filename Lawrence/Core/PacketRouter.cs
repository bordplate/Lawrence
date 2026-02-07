using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace Lawrence.Core;


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