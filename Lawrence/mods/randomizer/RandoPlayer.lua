RandoPlayer = class("RandoPlayer", Player)

function RandoPlayer:Made()
    self.damageCooldown = 0
    self.goldBoltCount = 0
    self.ready = false
    self.ingame = false
    
    self.lobby = null
    
    self.helga = null
    
    self.gameState = 0

    self.level_unlock_queue = {}
    self.item_unlock_queue = {}
    self.special_unlock_queue = {}
    
    self.race_position = 5
    
    self.skillpointCounters = {
        Player.offset.aridiaShipsKilled,
        Player.offset.eudoraShipsKilled,
        Player.offset.gasparShipsKilled,
        Player.offset.pokitaruShipsKilled,
        Player.offset.hovenShipsKilled,
        Player.offset.oltanisShipsKilled,
        Player.offset.veldin2CommandosKilled,
    }

    self.receivedItemsWhileLoading = false
    self.fullySpawnedIn = false
    
--     for _, counter in ipairs(self.skillpointCounters) do
--         self:MonitorAddress(counter, 4) 
--     end
-- 
--     for i = 2, 26 do
--         self:MonitorAddress(Player.offset.gildedItems + i, 1)
--     end
    
    self:MonitorAddress(Player.offset.goldBolts + 16 * 4 + 1, 1)
    self:MonitorAddress(Player.offset.has_zoomerator, 1)
    self:MonitorAddress(Player.offset.rilgar_race_pb, 4)
    self:MonitorAddress(Player.offset.race_position, 4)
    self.hasCollectedKaleboGrindrailBolt = false
end

function RandoPlayer:Start()
    if self.lobby.options.cheats.value then
        self.GhostRatchetLabel = Label:new("R1: Set Ghost Ratchet for 1 second", 250, 390, 0xC0FFA888, {GameState.Menu})
        self:AddLabel(self.GhostRatchetLabel)
    end
    
    self.TeleportToShipLabel = Label:new("\x13: Teleport to ship", 250, 370, 0xC0FFA888, {GameState.Menu})
    self:AddLabel(self.TeleportToShipLabel)
    
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
    if self.fullySpawnedIn and state == 0 and self.lobby.universe.got_novalis_mayor and self:Level():GetName() == "Novalis" then
        self:SetLevelFlags(1,1,0,{0xff})
    end
end

function RandoPlayer:OnControllerInputTapped(input)
    if self.gameState == 3 and input & 0x8 ~= 0 and self.lobby.options.cheats.value then -- R1
        self:SetGhostRatchet(200)
    end
    
    
    if input & 0x10 ~= 0 and self.fullySpawnedIn then
        self.lobby.universe.replacedMobys:Triangle(self)
    end

    if self.gameState == 3 and input & 0x80 ~= 0  then -- square
        self:TeleportToShip()
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
    --print("Address " .. address .. " changed from " .. oldValue .. " to " .. newValue)

    if address == Player.offset.goldBolts + 16 * 4 + 1 and newValue == 1 and not self.hasCollectedKaleboGrindrailBolt then
        self:OnCollectedGoldBolt(16, 1)
        self.hasCollectedKaleboGrindrailBolt = true
    end
    
    if address == Player.offset.has_zoomerator and newValue == 1 and self.lobby.universe.has_zoomerator == false then
        self:SetAddressValue(Player.offset.has_zoomerator, 0, 1)
    end

    if address == Player.offset.rilgar_race_pb and newValue ~= 0 and self.lobby.universe.has_zoomerator == true then
        if self.race_position == 1 then
            self:OnUnlockItem(0x30, false)
        else
            print("finished race but did not win... setting PB back to 0")
            self:SetAddressValue(Player.offset.rilgar_race_pb, 0, 4)
        end
    end

    if address == Player.offset.race_position then
        self.race_position = newValue
    end
end

function RandoPlayer:OnGiveBolts(boltDiff, totalBolts)
    self.lobby.universe:GiveBolts(boltDiff)
end

function RandoPlayer:OnRespawned()
    self.lobby.universe.replacedMobys:RemoveReplacedMobys(self)

    if self.receivedItemsWhileLoading then
        for _, planet in ipairs(self.level_unlock_queue) do
            print("Delayed unlocking planet: " .. tostring(planet))
            self:UnlockLevel(planet)
        end
        for _, item in ipairs(self.item_unlock_queue) do
            print("Delayed unlocking item: " .. tostring(item))
            self:GiveItem(item, IsGameItemStartingItem(item))
        end
        for _, special in ipairs(self.special_unlock_queue) do
            print("Delayed unlocking special: " .. tostring(special))
            self:SetAddressValue(special, 1, 1)
        end
        self.level_unlock_queue = {}
        self.item_unlock_queue = {}
        self.special_unlock_queue = {}
        self.receivedItemsWhileLoading = false
    end
    
    if not self.fullySpawnedIn then
        self.fullySpawnedIn = true
        if not self.lobby.universe.using_outdated_AP then
            self:SetAddressValue(Player.offset.challenge_mode, 1, 1)
        end
        PlayerResync(self.lobby.universe, self, self.lobby.universe.ap_client.ap.checked_locations)
        self:UpdateHPAmount()
    end
    self:SetBolts(self.lobby.universe.totalBolts)
    self:UpdateVendorContents()
    FixPlanetsForPlayer(self.lobby.universe, self)
    self:UpdateHPAmount()
end

--function RandoPlayer:OnLevelFlagChanged(flag_type, level, size, index, value)
--    print(string.format("OnLevelFlagChanged: type: %s, level: %s, size: %s, index: %s, value: %s", tostring(flag_type), tostring(level), tostring(size), tostring(index), tostring(value)))
--end

function Player:OnStartInLevelMovie(movie, levelId)
    if levelId == 18 and movie == 4 then
        self.lobby.universe.ap_client:WinGame()
    end
end

function RandoPlayer:UpdateVendorContents()
    num_buyable_weapons = #self.lobby.universe.buyable_weapons
    local any_items_in_vendor = false
    for i = 0, 11 do
        if i+1 <= num_buyable_weapons then
            self:SetAddressValue(Player.offset.vendorItems + i, self.lobby.universe.buyable_weapons[i+1], 1)
            any_items_in_vendor = true
        elseif i-num_buyable_weapons+1 <= #self.lobby.universe.buyable_ammo then
            self:SetAddressValue(Player.offset.vendorItems + i, self.lobby.universe.buyable_ammo[i-num_buyable_weapons+1]+64, 1)
            any_items_in_vendor = true
        else
            self:SetAddressValue(Player.offset.vendorItems + i, 0xff, 1)
        end
    end
    if any_items_in_vendor == false or -- literally nothing in the vendor
            num_buyable_weapons >= 12 then -- too many items, no room left for any ammo
        self:SetAddressValue(Player.offset.vendorItems + 0, 0x4a, 1) -- place bomb glove ammo to prevent crashes
    end
end

function RandoPlayer:UpdateHPAmount()
    hp = 4
    if self.lobby.universe.has_premium_nanotech then hp = hp + 1 end
    if self.lobby.universe.has_ultra_nanotech then hp = hp + 3 end
    self:SetAddressValue(0x71fb28, hp, 4)
end

function RandoPlayer:TeleportToShip()
    level = self:Level():GetName()

    if level == "Veldin1" then
        self:SetPosition(132.09, 115.480, 31.430)
    elseif level == "Novalis" then
        self:SetPosition(162.530, 136.393, 60.5)
    elseif level == "Aridia" then
        self:SetPosition(210.41, 170.35, 25.35)
    elseif level == "Kerwan" then
        self:SetPosition(263.982, 102.092, 54.5)
    elseif level == "Eudora" then
        self:SetPosition(220.250, 162.04, 56)
    elseif level == "Rilgar" then
        self:SetPosition(338.32, 110.8, 62.7)
    elseif level == "BlargStation" then
        if self.oClass == 0 then -- if not clank
            self:SetPosition(247.950, 148.68, 138.3)
        end
    elseif level == "Umbris" then
        self:SetPosition(264.55, 72.13, 45.77)
    elseif level == "Batalia" then
        self:SetPosition(151.52, 196.72, 37.83)
    elseif level == "Gaspar" then
        self:SetPosition(291.3, 392.3, 36.25)
    elseif level == "Orxon" then
        self:SetPosition(229.65, 203.13, 49.22)
    elseif level == "Pokitaru" then
        self:SetPosition(498.82, 406, 230)
    elseif level == "Hoven" then
        self:SetPosition(304.1, 303.24, 31.83)
    elseif level == "GemlikStation" then
        self:SetPosition(508.551, 391.111, 315.02)
    elseif level == "Oltanis" then
        self:SetPosition(255.79, 155.22, 47)
    elseif level == "Quartu" then
        self:SetPosition(308.6, 190, 32)
    elseif level == "KaleboIII" then
        self:SetPosition(146.43, 113.23, 128.3)
    elseif level == "DreksFleet" then
        self:SetPosition(500.833, 609.732, 152.5)
    elseif level == "Veldin2" then
        self:SetPosition(341.5, 632.7, 87.250)
    end
end

function RandoPlayer:OnDisconnect()
    if self.lobby ~= null then
        self.lobby:Leave(self)
    end
end