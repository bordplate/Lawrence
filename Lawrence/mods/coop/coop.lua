require 'CoopPlayer'
require 'LobbyListView'

local LobbyUniverse = class("LobbyUniverse", Universe)
function LobbyUniverse:initialize()
    Universe.initialize(self)
    
    self.lobbies = ObservableList({})
    
    self.firstPlayer = false
end
 
function LobbyUniverse:OnPlayerJoin(player)
    if not self.firstPlayer then
        self.firstPlayer = true
        
        self:NewLobby(player, "")
        return
    end
    
    local lobby = LobbyListView(player, self)
    player:ShowView(lobby)
end

function LobbyUniverse:NewLobby(host, password)
    local lobby = Lobby(host, password)
    self.lobbies:Add(lobby)
    
    lobby:Join(host)
    
    return lobby
end

function LobbyUniverse:RemoveLobby(lobby)
    self.lobbies:Remove(lobby)
end

function LobbyUniverse:OnTick()
    
end

lobbyUniverse = LobbyUniverse:new()
lobbyUniverse:Start(true)
