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

function Player:Unstuck()
    if self:Universe():LuaEntity().allowUnstuck then
        self:SetGhostRatchet(150)
    else
        self:ToastMessage("Unstuck is not allowed in this universe!", 100)
    end
end