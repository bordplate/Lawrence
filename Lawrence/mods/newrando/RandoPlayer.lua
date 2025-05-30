RandoPlayer = class("RandoPlayer", Player)

function RandoPlayer:Made()
    self.damageCooldown = 0
    self.goldBoltCount = 0
    self.ready = false
    self.ingame = false
    
    self.lobby = null
    
    self.helga = null
    
    self.gameState = 0
    
    self.totalBolts = 0
    
    self.skillpointCounters = {
        Player.offset.aridiaShipsKilled,
        Player.offset.eudoraShipsKilled,
        Player.offset.gasparShipsKilled,
        Player.offset.pokitaruShipsKilled,
        Player.offset.hovenShipsKilled,
        Player.offset.oltanisShipsKilled,
        Player.offset.veldin2CommandosKilled,
    }
    
    self.fullySpawnedIn = false
    
--     for _, counter in ipairs(self.skillpointCounters) do
--         self:MonitorAddress(counter, 4) 
--     end
-- 
--     for i = 2, 26 do
--         self:MonitorAddress(Player.offset.gildedItems + i, 1)
--     end
    
    self:MonitorAddress(Player.offset.goldBolts + 16 * 4 + 1, 1)
    self.hasCollectedKaleboGrindrailBolt = false
end

function RandoPlayer:Start()
    self.lobby.universe:AddEntity(self)
    self:LoadLevel(self.lobby.startPlanet)
end

function RandoPlayer:OnCollectedGoldBolt(planet, number)
    print("Player collected gold bolt on " .. planet .. " number: " .. number);
    
    self.lobby.universe:OnPlayerGetGoldBolt(self, planet, number)
    
    for _, player in ipairs(self:Universe():LuaEntity():FindChildren("Player")) do
        if player:GUID() ~= self:GUID() then
            player:SetAddressValue(Player.offset.goldBolts + planet * 4 + number, 1, 1)
            player:ToastMessage("\x0cGold Bolt\x08 acquired", 60*5)
        end
    end
end

function RandoPlayer:Unfreeze()
    self.state = 0
end

function RandoPlayer:OnTick()
    if not self.lobby.started then return end
    if not self.fullySpawnedIn then return end
    self.lobby.universe.replacedMobys:ToastMessage(self)
    
    if (self.damageCooldown > 0) then
        self.damageCooldown = self.damageCooldown - 1
    end
end

function RandoPlayer:OnGameStateChanged(state)
    self.gameState = state
end

function RandoPlayer:OnControllerInputTapped(input)
    if self.gameState == 3 and input & 0x20 ~= 0 then
        --if self:Username() == "panad" then
        --    print("Moving player")
        --    self:SetPosition(251, 278, 55.5)
        --end
    end

    if self.gameState == 3 and input & 0x8 ~= 0 and self.lobby.options.cheats then
        self:SetGhostRatchet(200)
    end
    
    if self.gameState == 3 and input & 0x80 ~= 0 then
        if self:Username() == "panad" then
            print("Moving players")
            local z_pos = self.z + 1
            for _, player in ipairs(self:Universe():LuaEntity():FindChildren("Player")) do
                if player:GUID() ~= self:GUID() then
                    print("Moved player " .. player:Username() .. " to " .. self.x .. ", " .. self.y .. ", " .. z_pos)
                    player:SetPosition(self.x, self.y, z_pos)
                    z_pos = z_pos + 1
                end
            end
        end
    end
    
    if input & 0x10 ~= 0 then
        self.lobby.universe.replacedMobys:Triangle(self)
    end
end

function RandoPlayer:OnUnlockItem(item_id, equip)
    item = Item.GetById(item_id)
    
    print("Unlocking item " .. item.name)
    
    self.lobby.universe:OnPlayerGetItem(self, item_id)
end

function RandoPlayer:OnUnlockLevel(level)
    print("OnUnlockLevel: " .. tostring(level))
    self.lobby.universe:OnPlayerGetPlanet(self, level)
end

-- function RandoPlayer:OnUnlockSkillpoint(skillpoint)
-- --     self.lobby:AddUnlockedSkillpoint(skillpoint)
--     
--     for _, player in ipairs(self:Universe():LuaEntity():FindChildren("Player")) do
--         player:UnlockSkillpoint(skillpoint)
--     end
-- end

function RandoPlayer:MonitoredAddressChanged(address, oldValue, newValue)
    print("Address " .. address .. " changed from " .. oldValue .. " to " .. newValue)

    if address == Player.offset.has_raritanium then
        print("has_raritanium changed from " .. tostring(oldValue) .. " to " .. tostring(newValue))
    end

    if address == Player.offset.goldBolts + 16 * 4 + 1 and newValue == 1 and not self.hasCollectedKaleboGrindrailBolt then
        self:OnCollectedGoldBolt(16, 1)
        self.hasCollectedKaleboGrindrailBolt = true
    end
end

function RandoPlayer:OnGiveBolts(boltDiff, totalBolts)
    self.totalBolts = totalBolts
    self.lobby.bolts = totalBolts
    for _, player in ipairs(self:Universe():LuaEntity():FindChildren("Player")) do
        if player ~= self then
            player:GiveBolts(boltDiff)
        end
    end
end

function RandoPlayer:OnRespawned()
    self.lobby.universe.replacedMobys:RemoveReplacedMobys(self)
    
    if not self.fullySpawnedIn then
        for _, planet in ipairs(self.lobby.universe.level_unlock_queue) do
            print("Delayed unlocking planet: " .. tostring(planet))
            self:UnlockLevel(planet)
        end
        for _, item in ipairs(self.lobby.universe.item_unlock_queue) do
            print("Delayed unlocking item: " .. tostring(item))
            self:GiveItem(item, true)
        end
        for _, special in ipairs(self.lobby.universe.special_unlock_queue) do
            print("Delayed unlocking special: " .. tostring(special))
            self:SetAddressValue(special, 1, 1)
        end
        self.fullySpawnedIn = true
        
        PlayerResync(self.lobby.universe, self, self.lobby.universe.ap_client.ap.checked_locations)
        self:UpdateHPAmount()
    end
    self:UpdateVendorContents()
    FixPlanetsForPlayer(self.lobby.universe, self)
end

--function RandoPlayer:OnLevelFlagChanged(flag_type, level, size, index, value)
--    print(string.format("OnLevelFlagChanged: type: %s, level: %s, size: %s, index: %s, value: %s", tostring(flag_type), tostring(level), tostring(size), tostring(index), tostring(value)))
--end

function RandoPlayer:NotifyLocationCollected(location_id)
    print(string.format("player %s got location: %d", self.username, location_id))
end

function RandoPlayer:UpdateVendorContents()
    num_buyable_weapons = #self.lobby.universe.buyable_weapons
    for i = 0, 11 do
        if i+1 <= num_buyable_weapons then
            self:SetAddressValue(Player.offset.vendorItems + i, self.lobby.universe.buyable_weapons[i+1], 1)
        elseif i-num_buyable_weapons+1 <= #self.lobby.universe.buyable_ammo then
            self:SetAddressValue(Player.offset.vendorItems + i, self.lobby.universe.buyable_ammo[i-num_buyable_weapons+1]+64, 1)
        else
            self:SetAddressValue(Player.offset.vendorItems + i, 0xff, 1)
        end
    end
end

function RandoPlayer:UpdateHPAmount()
    hp = 4
    if self.has_premium_nanotech then hp = hp + 1 end
    if self.has_ultra_nanotech then hp = hp + 3 end
    self:SetAddressValue(0x71fb28, hp, 4)
end

function RandoPlayer:OnDisconnect()
    if self.lobby ~= null then
        self.lobby:Leave(self)
    end
end