require 'Lobby.LobbyUniverse'
require 'Race.RaceUniverse'

local raceUniverse = RaceUniverse:new()

-- Make a lobby universe where players initially spawn. Instead of spawning directly into a race. 
local universe = LobbyUniverse:new({raceUniverse})

-- Start the lobby as primary universe.
-- This is the universe players are dropped into when they connect. 
universe:Start(true)