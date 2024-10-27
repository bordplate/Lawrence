TeamRunPlayer = class("TeamRunPlayer", Player)

function TeamRunPlayer:Made()
    self.damageCooldown = 0
    self.goldBoltCount = 0
    
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
end

function TeamRunPlayer:OnCollectedGoldBolt(planet, number)
    print("Player collected gold bolt on " .. planet .. " number: " .. number);

    self:Parent():BlockGoldBolt(planet, number)

    self.universe.blocked_bolts[#self.universe.blocked_bolts+1] = {planet, number}

    self.goldBoltCount = self.goldBoltCount + 1
end

function TeamRunPlayer:MonitoredAddressChanged(address, oldValue, newValue)
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
end

function TeamRunPlayer:OnAttack(moby)
    if self.damageCooldown <= 0 then
        moby:Damage(1)
        self.damageCooldown = 40
    end
end

function TeamRunPlayer:Unfreeze()
    self.state = 0
end

function TeamRunPlayer:OnTick()
    if (self.damageCooldown > 0) then
        self.damageCooldown = self.damageCooldown - 1
    end
end

function TeamRunPlayer:OnGameStateChanged(state)
    self.gameState = state
end

function TeamRunPlayer:OnControllerInputTapped(input)
    if self.gameState == 3 and input & 0x20 ~= 0 then
        self:SetPosition(0, 0, -10000)
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

function TeamRunPlayer:OnUnlockItem(item_id, equip)
    item = Item.GetById(item_id)
    
    print("Unlocking item " .. item.name)

    self:GiveItem(item.id, equip)

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

function TeamRunPlayer:OnUnlockLevel(level)
    Player.OnUnlockLevel(self, level)
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

function TeamRunPlayer:OnUnlockSkillpoint(skillpoint)
    for _, player in ipairs(self:Universe():LuaEntity():FindChildren("Player")) do
        player:UnlockSkillpoint(skillpoint)
    end
end


function TeamRunPlayer:OnGiveBolts(boltDiff, totalBolts)
    self.totalBolts = totalBolts
    for _, player in ipairs(self:Universe():LuaEntity():FindChildren("Player")) do
        if player ~= self then
            player:GiveBolts(boltDiff)
        end
    end
end