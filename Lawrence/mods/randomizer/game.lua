require 'Lobby.LobbyUniverse'
require 'Rando.RandoUniverse'

local RandomizerUniverse = RandoUniverse:new()

local lobbyUniverse = LobbyUniverse:new({RandomizerUniverse})

lobbyUniverse:Start(true)