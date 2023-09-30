require 'Rando.RandoPlayer'

RandoUniverse = class("RandoUniverse", Universe) -- should probably have subclasses of RandoUniverse that decide more specific behaviors (such as what items can be achieved)
                                                 -- or just change up the universe based on choice

function RandoUniverse:initialize()
    Universe.initialize(self)

    self.maxPlayers = 8
    self.playerCount = 0
end

function RandoUniverse:OnPlayerJoin(player)
    player = player:Make(RandoPlayer)
    player.randoUniverse = self
    player:LoadLevel("Novalis")
end
-- no clue what i need to do here yet
