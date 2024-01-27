using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;

namespace Lawrence.Core; 

public class DirectoryClient {
    private DateTime _lastDirectoryPing = DateTime.MinValue;
    private string _directoryIp;

    private Server _server;

    public DirectoryClient(string directoryIp, Server server) {
        _directoryIp = directoryIp;
        _server = server;
    }

    public void Start() {
        Thread thread = new Thread(Run);
        thread.Start();
    }
    
    private void Run() {
        UdpClient client = null;
        
        while (true) {
            if ((DateTime.Now - _lastDirectoryPing).TotalMinutes >= 1.0) {
                // We can't dispose the client right after sending async, and
                //  on Linux we can't reuse the client for some reason.
                // So we do this instead. 
                if (client != null) {
                    client.Dispose();
                }
                        
                client = new UdpClient(_directoryIp, 2407);
                // Count players
                int players = 0;
                foreach (Client c in _server.Clients()) {
                    if (!c.IsDisconnected() && !c.WaitingToConnect) {
                        players++;
                    }
                }
                        
                _lastDirectoryPing = DateTime.Now;

                string ipString = Settings.Default()
                    .Get<string>("Server.advertise_ip", _server.ListenAddress(), true);
                Packet packet = Packet.MakeRegisterServerPacket(ipString,
                    (ushort)_server.ListenPort(), (ushort)_server.MaxPlayers(), (ushort)players, _server.ServerName());

                List<byte> packetData = new List<byte>();
                
                (MPPacketHeader header, byte[] data) = packet.GetBytes();
                
                packetData.AddRange(Packet.StructToBytes(header, Packet.Endianness.BigEndian));
                packetData.AddRange(data);

                client.SendAsync(packetData.ToArray());
            }
                    
            Thread.Sleep(1000);
        }
    }
    
    public void ForceDirectorySync() {
        _lastDirectoryPing = DateTime.MinValue;
    }
}