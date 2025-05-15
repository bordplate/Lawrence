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
    
    self.vendorItems = {
        [0]=0xff,
        0xff,
        0xff,
        0xff,
        0xff,
        0xff,
        0xff,
        0xff,
        0xff,
        0xff,
        0xff,
    }
    
    self.fullySpawnedIn = false
    self.level_unlock_queue = {}
    self.item_unlock_queue = {}
    self.special_unlock_queue = {}
    
--     for _, counter in ipairs(self.skillpointCounters) do
--         self:MonitorAddress(counter, 4) 
--     end
-- 
--     for i = 2, 26 do
--         self:MonitorAddress(Player.offset.gildedItems + i, 1)
--     end

       for i = 0, 11 do
           self:MonitorAddress(Player.offset.vendorItems + i, 1)
       end
   
       self:MonitorAddress(0x0096bff1, 1) -- has_raritanium
end

function RandoPlayer:Start()
    self.ingame = true
    self.ready = true

    if self.lobby.options.debugStart.value then
        self:GiveItem(Item.GetByName("Heli-pack").id)
        self:GiveItem(Item.GetByName("Thruster-pack").id)
        self:GiveItem(Item.GetByName("Hydro-pack").id)
        self:GiveItem(Item.GetByName("Sonic Summoner").id)
        self:GiveItem(Item.GetByName("O2 Mask").id)
        self:GiveItem(Item.GetByName("Pilot's Helmet").id)
        self:GiveItem(Item.GetByName("Swingshot").id)
        self:GiveItem(Item.GetByName("Hydrodisplacer").id)
        self:GiveItem(Item.GetByName("Trespasser").id)
        self:GiveItem(Item.GetByName("Metal Detector").id)
        self:GiveItem(Item.GetByName("Hologuise").id)
        self:GiveItem(Item.GetByName("PDA").id)
        self:GiveItem(Item.GetByName("Magneboots").id)
        self:GiveItem(Item.GetByName("Grindboots").id)
        self:GiveItem(Item.GetByName("Devastator").id)
        self:GiveItem(Item.GetByName("Visibomb").id)
        self:GiveItem(Item.GetByName("Taunter").id)
        self:GiveItem(Item.GetByName("Blaster").id)
        self:GiveItem(Item.GetByName("Pyrociter").id)
        self:GiveItem(Item.GetByName("Mine Glove").id)
        self:GiveItem(Item.GetByName("Walloper").id)
        self:GiveItem(Item.GetByName("Tesla Claw").id)
        self:GiveItem(Item.GetByName("Glove of Doom").id)
        self:GiveItem(Item.GetByName("Drone Device").id)
        self:GiveItem(Item.GetByName("Decoy Glove").id)
        self:GiveItem(Item.GetByName("Bomb Glove").id)
        self:GiveItem(Item.GetByName("Suck Cannon").id)
        self:GiveItem(Item.GetByName("Morph-o-Ray").id)
        self:GiveItem(Item.GetByName("R.Y.N.O.").id)
        
        self:SetBolts(150000)

        self:UnlockLevel(1)
        self:UnlockLevel(2)
        self:UnlockLevel(3)
        self:UnlockLevel(4)
        self:UnlockLevel(5)
        self:UnlockLevel(6)
        self:UnlockLevel(7)
        self:UnlockLevel(8)
        self:UnlockLevel(9)
        self:UnlockLevel(10)
        self:UnlockLevel(11)
        self:UnlockLevel(12)
        self:UnlockLevel(13)
        self:UnlockLevel(14)
        self:UnlockLevel(15)
        self:UnlockLevel(16)
        self:UnlockLevel(17)
        self:UnlockLevel(18)
    end

    if not self.lobby.started then
        self:LoadLevel(self.lobby.options.startPlanet.value)
    else
        self:SetBolts(self.lobby.bolts)

        for i, item in ipairs(self.lobby.unlockedItems) do
            self:GiveItem(item, false)
        end

        for i, infobot in ipairs(self.lobby.unlockedInfobots) do
            self:UnlockLevel(infobot)
        end

        for i, skillpoint in ipairs(self.lobby.unlockedSkillpoints) do
            self:UnlockSkillpoint(skillpoint)
        end
        
        -- Load last unlocked level
        self:LoadLevel(self.lobby.unlockedInfobots[#self.lobby.unlockedInfobots])
    end
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

function RandoPlayer:OnAttack(moby, sourceOClass, damage)
    if not self.lobby.options.friendlyFire.value then
        return
    end
    
    if self.damageCooldown <= 0 then
        moby:Damage(1)
        self.damageCooldown = 40
    end
end

function RandoPlayer:Unfreeze()
    self.state = 0
end

function RandoPlayer:OnTick()
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
        if self:Username() == "panad" then
            print("Moving player")
            self:SetPosition(168, 204, 42)
            --self:SetAddressValue(0x969EAC, 100, 4)
        end
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
    
    self:RemoveItemFromVendor(item_id)
    
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
       
    if address >= Player.offset.vendorItems and address <= Player.offset.vendorItems + 11 then
        self.vendorItems[address - Player.offset.vendorItems] = newValue
        print(tostring(self.vendorItems[0]) .. ", " .. table.concat(self.vendorItems, ", "))
    end

    if address == 0x0096bff1 then
        print("has_raritanium changed from " .. tostring(oldValue) .. " to " .. tostring(newValue))
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
        for _, planet in ipairs(self.level_unlock_queue) do
            print("Delayed unlocking planet: " .. tostring(planet))
            self:UnlockLevel(planet)
        end
        for _, item in ipairs(self.item_unlock_queue) do
            print("Delayed unlocking item: " .. tostring(item))
            self:GiveItem(item, true)
        end
        for _, special in ipairs(self.special_unlock_queue) do
            print("Delayed unlocking special: " .. tostring(special))
            player:SetAddressValue(special, 1, 1)
        end
        self.fullySpawnedIn = true
        self.level_unlock_queue = {}
        self.item_unlock_queue = {}
        self.special_unlock_queue = {}
    end
end

function RandoPlayer:OnLevelFlagChanged(flag_type, level, size, index, value)
    if (index == 3 and flag_type == 1) or (index == 44 and flag_type == 2) or (index == 4 and flag_type == 1) or (index == 0 and flag_type == 1) then
    else
        print(string.format("OnLevelFlagChanged: type: %s, level: %s, size: %s, index: %s, value: %s", tostring(flag_type), tostring(level), tostring(size), tostring(index), tostring(value)))
    end
end

function RandoPlayer:NotifyLocationCollected(location_id)
    print(string.format("player %s got location: %d", self.username, location_id))
end

function RandoPlayer:RemoveItemFromVendor(item_id)
    for i = 0, #self.vendorItems do
        if self.vendorItems[i] == item_id then
            -- remove and shift all the items after it one to the left
            for j = i, (#self.vendorItems - 1) do
                self.vendorItems[j] = self.vendorItems[j+1]
                self:SetAddressValue(Player.offset.vendorItems + j, self.vendorItems[j+1], 1)
            end
        end
    end
end

function RandoPlayer:OnDisconnect()
    if self.lobby ~= null then
        self.lobby:Leave(self)
    end
end

-- novalis bridge open cutscene play
--self:SetAddressValue(0x44393bc0 + 0xbc, 0x1, 1) -- camera ???
--self:SetAddressValue(0x443d4100 + 0x2c, 0x000000b4, 4) -- play bridge cutscene