require 'Entity'

Player = class("Player", Entity)

function Player:initialize(internalEntity)
    Entity.initialize(self, internalEntity)
end