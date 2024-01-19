require 'Lobby.LobbyUniverse'
require 'Main.HASUniverse'

-- Make a lobby universe where players initially spawn. Instead of spawning directly into a game. 
local universe = LobbyUniverse:new()

-- Start the lobby as primary universe.
-- This is the universe players are dropped into when they connect. 
universe:Start(true)