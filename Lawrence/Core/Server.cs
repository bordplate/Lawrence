using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Lawrence.Core; 

/// <summary>
/// Responsible for accepting new clients and running and progressing the game loop.
/// </summary>
public class Server {
    const double TargetTickDurationMs = 1000.0 / 60.0; // 16.67 ms per tick for 60 ticks per second
    
    private long _tickCount = 0;             // Total number of ticks
    private DateTime _lastAverageUpdateTime = DateTime.UtcNow;
    private long _ticksPerSecond = 0;
    
    private readonly Mutex _clientMutex = new();

    private readonly List<Client> _clients = new();

    private readonly UdpClient _udpServer;

    private Thread? _clientThread;
    private Thread? _runThread;

    private readonly string _serverName;
    private readonly int _maxPlayers;

    private bool _processTicks = true;

    private static ulong _time;

    public Server(string listenAddress, int port, string serverName = "<Empty server name>", int maxPlayers = 20) {
        IPEndPoint ipep = new IPEndPoint(IPAddress.Parse(listenAddress), port);
        _udpServer = new UdpClient(ipep);
        
        _serverName = serverName;
        _maxPlayers = maxPlayers;
    }
    
    public void SetProcessTicks(bool processTicks) {
        _processTicks = processTicks;
    }
    
    public string? ListenAddress() {
        return ((IPEndPoint?)_udpServer.Client.LocalEndPoint)?.Address.ToString();
    }

    public int ListenPort() {
        return ((IPEndPoint?)_udpServer.Client.LocalEndPoint)?.Port ?? 0;
    }
    
    public string ServerName() {
        return _serverName;
    }
    
    public int MaxPlayers() {
        return _maxPlayers;
    }
    
    public static void Tick() {
        Game.Game.Shared().Tick();
    }
    
    /// <summary>
    /// Starts the two threads that run the server. One for accepting new clients and one for running the game loop.
    /// </summary>
    public void Start() {
        _clientThread = new Thread(AcceptClients);
        _clientThread.Start();
        
        _runThread = new Thread(Run);
        _runThread.Start();
    }
    
    /// <summary>
    /// Accepts new clients and adds them to the clients list.
    /// </summary>
    private void AcceptClients() {
        while (true) {
            if (_udpServer.Available <= 0) {
                Thread.Sleep(1);
                continue;
            }

            try {
                IPEndPoint? clientEndpoint = null;
                var data = _udpServer.Receive(ref clientEndpoint);

                if (data.Length <= 0) {
                    Logger.Error("Hey, why is it 0?");
                    continue;
                }

                bool existingPlayer = false;
                 
                for (int i = _clients.Count - 1; i >= 0; i--) {
                    Client p = _clients[i];

                    if (p.IsDisconnected()) {
                        _clientMutex.WaitOne();
                        _clients.RemoveAt(i);
                        _clientMutex.ReleaseMutex();
                        
                        continue;
                    }

                    if (!p.IsDisconnected() && p.GetEndpoint().Address.Equals(clientEndpoint.Address) && p.GetEndpoint().Port == clientEndpoint.Port) {
                        p.ReceiveData(data);
                        existingPlayer = true;
                    }
                }

                if (!existingPlayer) {
                    NewClient(clientEndpoint, data);
                }
            }
            catch (SocketException e)
            {
                Logger.Error($"Receive error", e);
            }
        }
    }
    
    /// <summary>
    /// Runs the game loop. This is where the game progresses.
    /// </summary>
    private void Run() {
        Stopwatch watch = new Stopwatch();
        watch.Start();
        
        while (true) {
            DateTimeOffset unixEpoch = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);
            TimeSpan timeSinceEpoch = DateTimeOffset.UtcNow - unixEpoch;
            _time = (ulong)timeSinceEpoch.TotalMilliseconds;
            
            _clientMutex.WaitOne();
                
            // Clients that are waiting to connect aren't part of a `Player` tick loop so the Client tick 
            //   isn't being called. Therefore we call the Tick function here if it's waiting to connect, so
            //   packets can be processed like they should on the right thread. 
            foreach (Client client in _clients) {
                if (client.WaitingToConnect) {
                    if (client.GetInactiveSeconds() > 10) {
                        client.Disconnect();
                        continue;
                    }
                    
                    client.Tick();
                }
            }
                
            _clientMutex.ReleaseMutex();
            
            if (_processTicks) {
                Tick();
            }
            
            watch.Stop();
                
            double tickDuration = watch.ElapsedMilliseconds;
            
            _tickCount++;

            if (tickDuration > TargetTickDurationMs+1) {
                Logger.Trace($"Tick running late: Elapsed={tickDuration}ms");
            } else {
                int sleepTime = (int)TargetTickDurationMs - (int)tickDuration;
                if (sleepTime > 0) {
                    Thread.Sleep(sleepTime);
                }
            }

            // Update average ticks per second every second
            if ((DateTime.UtcNow - _lastAverageUpdateTime).TotalSeconds >= 1.0) {
                _ticksPerSecond = _tickCount;

                // Reset the counters
                _tickCount = 0;
                _lastAverageUpdateTime = DateTime.UtcNow;
            }

            watch.Restart();
        }
    }
    
    /// <summary>
    /// Creates a new client and adds it to the clients list.
    /// </summary>
    /// <param name="endpoint"></param>
    /// <param name="data"></param>
    void NewClient(IPEndPoint endpoint, byte[]? data = null) {
        Logger.Log($"Connection from {endpoint}");

        _clientMutex.WaitOne();
        
        int index = _clients.Count;
        for (int i = 0; i < _clients.Count; i++) {
            if (_clients[i].IsDisconnected()) {
                index = i;
                break;
            }
        }

        Client client = new Client(endpoint, (uint)index, this);

        if (index >= _clients.Count) {
            _clients.Add(client);
        } else {
            _clients[index] = client;
        }

        // Receive their first packet
        if (data != null) {
            client.ReceiveData(data);
        }
        
        _clientMutex.ReleaseMutex();
    }

    public void SendTo(byte[] bytes, EndPoint endpoint) {
        try {
            _udpServer.Client.SendTo(bytes, endpoint);
        } catch (Exception e) {
            Logger.Error($"Error sending packet: {e.Message}");
        }
    }

    public List<Client> Clients() {
        return _clients;
    }
    
    public long TicksPerSecond() {
        return _ticksPerSecond + 1;
    }

    public static ulong Time() {
        return _time;
    }
}