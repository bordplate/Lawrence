require 'Lobby.LobbyPlayer'


LobbyUniverse = class("LobbyUniverse", Universe)

function LobbyUniverse:initialize(raceUniverses)
    Universe.initialize(self)

    self.races = raceUniverses
end

-- When a new player joins this Universe. 
function LobbyUniverse:OnPlayerJoin(player)
    player:LoadLevel("KaleboIII")
    player:Make(LobbyPlayer)
end

function LobbyUniverse:OnTick()
    local players = self:FindChildren("Player")

    if #players <= 0 then
        return
    end

    --for i, race in ipairs(self.races) do
    --    if race.playerCount < race.maxPlayers then
    --        for j, player in ipairs(players) do
    --            race:AddEntity(player)
    --            goto break_player_add
    --        end
    --
    --        ::break_player_add::
    --    end
    --end
end