using System;

namespace Lawrence.Core.Middleware;

public class AckMiddleware: IMiddleware {
    public void Execute(PacketContext context, Action<PacketContext> next) {
        var header = context.Header;
        var client = context.Client;
        
        // If this packet requires ack and is not RPC, we send ack before processing.
        // If this is RPC we only send ack here if a previous ack has been sent and cached.
        if (header.PacketType != MPPacketType.MP_PACKET_ACK && header.RequiresAck != 0) {
            // We don't ack ack messages
            // If this is an RPC packet and we've already processed and cached it, we use the cached response. 
            if ((header.Flags & MPPacketFlags.MP_PACKET_FLAG_RPC) != 0 &&
                client.Acked[header.RequiresAck].AckCycle == header.AckCycle) {
                client.Server.SendTo(client.Acked[header.RequiresAck].Packet, client.GetEndpoint());
                
                return;
            }
            
            if ((header.Flags & MPPacketFlags.MP_PACKET_FLAG_RPC) == 0) {
                // If it's not RPC, we just ack the packet and process the packet
                MPPacketHeader ack = new MPPacketHeader {
                    PacketType = MPPacketType.MP_PACKET_ACK,
                    Flags = 0,
                    Size = 0,
                    RequiresAck = header.RequiresAck,
                    AckCycle = header.AckCycle
                };

                client.SendPacket(ack, null);
            }
            
            if ((header.Flags & MPPacketFlags.MP_PACKET_FLAG_RPC) != 0) {
                context.HaveRepliedToRpc = false;
            }
        }
        
        next(context);

        if (!context.HaveRepliedToRpc) {
            throw new Exception("Server failed to reply to a RPC packet!");
        }
    }
}