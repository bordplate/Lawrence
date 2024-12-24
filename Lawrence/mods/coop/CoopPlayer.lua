require 'TestMoby'

CoopPlayer = class("CoopPlayer", Player)

function CoopPlayer:Made()
    self.damageCooldown = 0
    self.goldBoltCount = 0
    self.ready = false
    self.ingame = false
    
    self.lobby = null
    
    self.clanky = null
    
    self.gameState = 0
    
    self.skillpointCounters = {
        Player.offset.aridiaShipsKilled,
        Player.offset.eudoraShipsKilled,
        Player.offset.gasparShipsKilled,
        Player.offset.pokitaruShipsKilled,
        Player.offset.hovenShipsKilled,
        Player.offset.oltanisShipsKilled,
        Player.offset.veldin2CommandosKilled,
    }
    
    for _, counter in ipairs(self.skillpointCounters) do
        self:MonitorAddress(counter, 4)
    end

    for i = 2, 26 do
        self:MonitorAddress(Player.offset.gildedItems + i, 1)
    end
end

function CoopPlayer:Start()
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
        self:UnlockAllGoldBolts()
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

function CoopPlayer:OnCollectedGoldBolt(planet, number)
    print("Player collected gold bolt on " .. planet .. " number: " .. number);
    
    for _, player in ipairs(self:Universe():LuaEntity():FindChildren("Player")) do
        if player:GUID() ~= self:GUID() then
            player:SetAddressValue(Player.offset.goldBolts + planet * 4 + number, 1, 1)
            player:ToastMessage("\x0cGold Bolt\x08 acquired", 60*5)
        end
    end
end

function CoopPlayer:UnlockAllGoldBolts()
    for i = 0, 17 do
        for j = 0, 3 do
            self:SetAddressValue(Player.offset.goldBolts + i * 4 + j, 1, 1)
        end
    end
end

function CoopPlayer:MonitoredAddressChanged(address, oldValue, newValue)
    print("Address " .. address .. " changed from " .. oldValue .. " to " .. newValue)
    
    local addressIsSkillpointCounter = false
    for _, counter in ipairs(self.skillpointCounters) do
        if address == counter then
            addressIsSkillpointCounter = true
            break
        end
    end
    
    if addressIsSkillpointCounter and newValue > oldValue then
        for _, player in ipairs(self:Universe():LuaEntity():FindChildren("Player")) do
            if player:GUID() ~= self:GUID() then
                player:SetAddressValue(address, newValue, 4)
            end
        end
    end
    
    if address >= Player.offset.gildedItems and address <= Player.offset.gildedItems + 35 then
        local itemIndex = address - Player.offset.gildedItems + 2
        print("Item " .. itemIndex .. " changed to " .. newValue)
        
        if newValue ~= 0 then
            for _, player in ipairs(self:Universe():LuaEntity():FindChildren("Player")) do
                if player:GUID() ~= self:GUID() then
                    player:SetAddressValue(address, 1, 1)
                end
            end
        end
    end
end

function CoopPlayer:OnAttack(moby, sourceOClass, damage)
    if not self.lobby.options.friendlyFire.value then
        return
    end
    
    if self.damageCooldown <= 0 then
        moby:Damage(1)
        self.damageCooldown = 40
    end
end

function CoopPlayer:Unfreeze()
    self.state = 0
end

function CoopPlayer:OnTick()
    --if self.clanky == null then
    --    self.clanky = self:SpawnInstanced(TestMoby)
    --    self.clanky:SetPosition(self.x, self.y, self.z + 2)
    --end
    --
    --self.clanky:SetPosition(self.x, self.y, self.z + 2)
    
    if (self.damageCooldown > 0) then
        self.damageCooldown = self.damageCooldown - 1
    end
end

function CoopPlayer:OnGameStateChanged(state)
    self.gameState = state
end

function CoopPlayer:OnControllerInputTapped(input)
    if self.gameState == 3 and input & 0x20 ~= 0 then
        self:SetPosition(0, 0, -10000)
        --self:SetAddressValue(0x969EAC, 100, 4)
    end
    
    if self.gameState == 3 and input & 0x80 ~= 0 then
        if self:Username() == "bordplate3" then
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
end

function CoopPlayer:OnUnlockItem(item_id, equip)
    item = Item.GetById(item_id)
    
    print("Unlocking item " .. item.name)

    self:GiveItem(item.id, equip)
    
    self.lobby:AddUnlockedItem(item.id)

    for _, player in ipairs(self:Universe():LuaEntity():FindChildren("Player")) do
        if player:Username() ~= self:Username() then
            if item.isWeapon then
                player:ToastMessage("You've purchased a \x0c" .. item.name .. "\x08!", 60*5)
            end
            
            print("Giving " .. item.name .. " to player " .. player:Username())
            player:GiveItem(item.id, false)

            if player:Level() == self:Level() then
                if item.name == "Swingshot" then
                    player:DeleteAllChildrenWithOClass(890)  -- Delete Helga
                end
                if item.name == "Grindboots" then
                    player:DeleteAllChildrenWithOClass(1190)  -- Delete Fred
                end
                if item.name == "Hydrodisplacer" then
                    player:DeleteAllChildrenWithOClass(1016)  -- Delete Hydrodisplacer
                end
                if item.name == "Metal Detector" then
                    player:DeleteAllChildrenWithOClass(1283)  -- Delete Plumber
                end
            end
        end
    end
end

function CoopPlayer:OnUnlockLevel(level)
    Player.OnUnlockLevel(self, level)
    
    self.lobby:AddUnlockedInfobot(level)
    
    for _, player in ipairs(self:Universe():LuaEntity():FindChildren("Player")) do
        player:UnlockLevel(level)

        if player:Level() == self:Level() then
            if level == 2 then  -- Aridia
                player:DeleteAllChildrenWithOClass(774)  -- Delete Plumber
            end
            if level == 6 then  -- Blarg
                player:DeleteAllChildrenWithOClass(1190)  -- Delete Lietuenant
            end
            if level == 5 then  -- Rilgar
                player:DeleteAllChildrenWithOClass(750)  -- Delete infobot
            end
            if level == 7 then  -- Umbris
                player:DeleteAllChildrenWithOClass(919)  -- Delete Bouncer
            end
            if level == 8 then  -- Batalia
                player:DeleteAllChildrenWithOClass(750)  -- Delete infobot
            end
            if level == 9 then  -- Gaspar
                player:DeleteAllChildrenWithOClass(1144)  -- Delete Deserter
            end
            if level == 10 then  -- Orxon
                player:DeleteAllChildrenWithOClass(1130)  -- Delete Commando
            end
        end
    end
end

function CoopPlayer:OnUnlockSkillpoint(skillpoint)
    self.lobby:AddUnlockedSkillpoint(skillpoint)
    
    for _, player in ipairs(self:Universe():LuaEntity():FindChildren("Player")) do
        player:UnlockSkillpoint(skillpoint)
    end
end


function CoopPlayer:OnGiveBolts(boltDiff, totalBolts)
    self.totalBolts = totalBolts
    self.lobby.bolts = totalBolts
    for _, player in ipairs(self:Universe():LuaEntity():FindChildren("Player")) do
        if player ~= self then
            player:GiveBolts(boltDiff)
        end
    end
end

function CoopPlayer:OnDisconnect()
    if self.lobby ~= null then
        self.lobby:Leave(self)
    end
end
