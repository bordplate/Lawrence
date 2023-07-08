require 'RaceUniverse'

----
-- Game mode functions
----

local LobbyUniverse = class("LobbyUniverse", Universe)
function LobbyUniverse:initialize()
    Universe.initialize(self)

    local raceUniverse = RaceUniverse:new()
    raceUniverse:Start(false)

    self.races = { raceUniverse }
end

-- When a new player joins this Universe. 
function LobbyUniverse:OnPlayerJoin(player)
    player:LoadLevel("Eudora")
end

function LobbyUniverse:OnTick()
    local players = self:FindChildren("Player")

    if #players <= 0 then
        return
    end

    for i, race in ipairs(self.races) do
        if race.playerCount < race.maxPlayers then
            for j, player in ipairs(players) do
                race:AddEntity(player)
                goto break_player_add
            end

            ::break_player_add::
        end
    end
end

local universe = LobbyUniverse:new()

-- Start the Universe as primary universe.
-- Primary universes handle player join notifications. 
universe:Start(true)