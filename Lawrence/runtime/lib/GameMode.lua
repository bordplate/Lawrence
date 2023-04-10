require 'middleclass'

GameMode = class('GameMode')

-- Initialize by making a new entity in the game and storing the internal C# object. 
function GameMode:initialize()
    self.Active = false
end

function GameMode:Start()
    Game:StartGame(self)
    self.Active = true
end

-- Called every tick of the game
function GameMode:OnTick()
    
end
