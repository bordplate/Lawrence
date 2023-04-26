require 'middleclass'

Universe = class('Universe', Entity)

-- Initialize by making a new entity in the game and storing the internal C# object. 
function Universe:initialize()
    local universeEntity = Game:NewUniverse(self)

    Entity.initialize(self, universeEntity)


end

-- Called every tick of the game
function Universe:OnTick()
    
end
