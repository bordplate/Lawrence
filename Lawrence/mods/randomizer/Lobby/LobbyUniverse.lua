require 'Lobby.LobbyPlayer'

LobbyUniverse = class("LobbyUniverse", Universe)

function LobbyUniverse:initialize(randoUniverses)
    Universe.initialize(self)

    self.randos = randoUniverses
end

-- When a new player joins this Universe. 
function LobbyUniverse:OnPlayerJoin(player)
    print("Player is joining Lobby")
    player:LoadLevel("KaleboIII")
    player:Make(LobbyPlayer)
end

function LobbyUniverse:OnTick()
    local players = self:FindChildren("Player")

    if #players <= 0 then
        return
    end
end