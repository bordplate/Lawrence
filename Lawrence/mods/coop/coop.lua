require 'CoopPlayer'
require 'LobbyListView'

local LobbyUniverse = class("LobbyUniverse", Universe)
function LobbyUniverse:initialize()
    Universe.initialize(self)
    
    self.lobbies = ObservableList({})
end
 
function LobbyUniverse:OnPlayerJoin(player)
    local lobby = LobbyListView(player, self)
    player:AddEntity(lobby)
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
    for _, lobby in ipairs(self.lobbies) do
        if #lobby.players <= 0 then
            if lobby.inactiveTimer > 60 * 60 * 15 then  -- Keep lobbies open for 15 minutes
                print("Deleting lobby " .. lobby.lobbyName .. " due to inactivity")
                self:RemoveLobby(lobby)
                lobby.universe:Delete()
            end

            lobby.inactiveTimer = lobby.inactiveTimer + 1
        end
    end
end

lobbyUniverse = LobbyUniverse:new()
lobbyUniverse:Start(true)
