require 'Entity'

Level = class("Level", Entity)

function Level:initialize(internalEntity)
    Entity.initialize(self, internalEntity)
end
