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

function Player:OnUnlockItem(item, equip)
    self:GiveItem(item, equip)
end

function Player:OnUnlockLevel(level)
    self:UnlockLevel(level)
end

function Player:OnRespawned()
    self:SetGhostRatchet(150)
end