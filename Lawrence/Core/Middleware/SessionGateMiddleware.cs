using System;

namespace Lawrence.Core.Middleware;

public class SessionGateMiddleware: IMiddleware {
    public void Execute(PacketContext context, Action<PacketContext> next) {
        var client = context.Client;
        var header = context.Header;
        // Check if handshake is complete, otherwise start completing it.

        if (context.RequiresDirectoryMode && !Lawrence.DirectoryMode()) {
            throw new Exception("You can't send this type of packet to this server.");
        }

        // TODO: Implement some sort of protection against anyone spoofing others.
        //       Ideally the handshake should start a session that the client can
        //          easily use to identify itself. Ideally without much computational
        //          overhead. 
        if ((!client.HandshakeCompleted || client.WaitingToConnect) && context.RequiresHandshake) {
            // Client has sent a packet that is not a handshake packet.
            // We tell the client we don't know it and it should reset state and
            // start handshake. 
            client.SendPacket(new MPPacketHeader { PacketType = MPPacketType.MP_PACKET_IDKU }, null);
            return;
        }

        next(context);
    }
}