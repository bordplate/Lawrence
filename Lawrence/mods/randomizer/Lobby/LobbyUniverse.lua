require 'Lobby.LobbyPlayer'

LobbyUniverse = class("LobbyUniverse", Universe)

function LobbyUniverse:initialize(randoUniverses)
    Universe.initialize(self)

    self.randos = randoUniverses
end

-- When a new player joins this Universe. 
function LobbyUniverse:OnPlayerJoin(player)
    print("Player is joining Lobby")
    --player:LoadLevel("KaleboIII")
    player:LoadLevel("Veldin1")
    player:Make(LobbyPlayer)
    player:setCommunicationFlags(Player.communicationFlags.ENABLE_ALL - Player.communicationFlags.ENABLE_ON_UNLOCK_ITEM)
end

function LobbyUniverse:OnTick()
    local players = self:FindChildren("Player")

    if #players <= 0 then
        return
    end
end