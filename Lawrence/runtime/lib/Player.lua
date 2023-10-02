require 'Entity'

Player = class("Player", Entity)

function Player:initialize(internalEntity)
    Entity.initialize(self, internalEntity)
end

function Player:SpawnInstanced(mobyType)
    local moby = self:Level():SpawnMoby(mobyType)
    moby:SetInstanced(true)
    self:AddEntity(moby)
    
    return moby
end

function Player:OnUnlockItem(item)
    self:GiveItem(item)
end

function Player:OnUnlockPlanet(planet)
    self:UnlockPlanet(planet)
end