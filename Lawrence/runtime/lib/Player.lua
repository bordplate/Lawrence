require 'Entity'

Player = class("Player", Entity)

Player.offset = {
    aridiaShipsKilled = 0x96c9dc,
    eudoraShipsKilled = 0x96c9e0,
    gasparShipsKilled = 0x96c9e4,
    pokitaruShipsKilled = 0x96c9e8,
    hovenShipsKilled = 0x96c9ec,
    oltanisShipsKilled = 0x96c9f0,
    veldin2CommandosKilled = 0x96c9f8,
}

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

function Player:OnGiveBolts(boltDiff, totalBolts)
    -- do nothing, the client merely notified the server that it received bolts
end

function Player:Unstuck()
    if self:Universe():LuaEntity().allowUnstuck then
        self:SetGhostRatchet(150)
    else
        self:ToastMessage("Unstuck is not allowed in this universe!", 100)
    end
end