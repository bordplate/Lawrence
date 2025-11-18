require 'Entity'

Player = class("Player", Entity)

Player.offset = {
    goldBolts = 0xa0ca34,
    aridiaShipsKilled = 0x96c9dc,
    eudoraShipsKilled = 0x96c9e0,
    gasparShipsKilled = 0x96c9e4,
    pokitaruShipsKilled = 0x96c9e8,
    hovenShipsKilled = 0x96c9ec,
    oltanisShipsKilled = 0x96c9f0,
    veldin2CommandosKilled = 0x96c9f8,
    gildedItems = 0x969ca8,
    vendorItems = 0x71fb2c,
    
    has_zoomerator = 0x96bff0,
    has_raritanium = 0x96bff1,
    has_codebot = 0x96bff2,
    has_premium_nanotech = 0x96bff4,
    has_ultra_nanotech = 0x96bff5,
    
    rilgar_race_pb = 0xa0cd04,
    race_position = 0x96a5a0,
    
    challenge_mode = 0x96c9fc,
}

Player.communicationFlags = {
    ENABLE_ON_UNLOCK_ITEM     =0x00000001,
    ENABLE_ON_UNLOCK_LEVEL    =0x00000002,
    ENABLE_ON_PICKUP_GOLD_BOLT=0x00000004,
    ENABLE_ON_GET_BOLTS       =0x00000008,

    ENABLE_ALL=                0xffffffff
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

function Player:ShowView(view)
    self._internalEntity:ShowView(view._internalEntity)
end

function Player:OnUnlockItem(item, equip)
    self:GiveItem(item, equip)
end

function Player:OnStartInLevelMovie(movie, levelId)
    
end

function Player:OnUnlockLevel(level)
    self:UnlockLevel(level)
end

function Player:OnUnlockSkillpoint(skillpoint)
    self:UnlockSkillpoint(skillpoint)
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

function Player:OnDisconnect()
    
end
