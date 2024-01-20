using System.Threading;

namespace Lawrence.Core; 

public class DirectoryServer {
    private readonly Server _server;
    private readonly ServerDirectory _directory;

    public DirectoryServer(string listenAddress, int port) {
        _server = new Server(listenAddress, port);
        _server.SetProcessTicks(false);
        
        _directory = new();
    }
    
    public ServerDirectory Directory() {
        return _directory;
    }
    
    public string ListenAddress() {
        return _server.ListenAddress();
    }

    public Server Server() {
        return _server;
    }

    public void Start() {
        _server.Start();
    }
}