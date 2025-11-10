require 'ReplacementMobys.ReplacementMobys'
require 'APClient'
require 'Locations'
require 'LocationSyncing'
require 'Items'
require 'runtime.levels.Common.Button'
RandoUniverse = class("RandoUniverse", Universe)

-- global to this mod
local game_name = "Ratchet & Clank"
local items_handling = 7  -- full remote

-- TODO: user input
local host = "localhost"
local slot = "Player1"
local password = ""

function RandoUniverse:initialize(lobby)
    Universe.initialize(self)
    
    self.replacedMobys = ReplacementMobys(self)
    
    self.lobby = lobby
    
    self.ap_client = nil
    self.ap_client_initialized = false
    
    self.buyable_weapons = {}
    self.buyable_ammo = {0x0a} -- list weapons, +64 is performed to turn it into ammo (if we truly have no ammo for sale, PDA will crash the game)
    self.gotten_first_ammo_weapon = false
    self.already_bought_weapons = {}

    self.has_hoverboard = false
    self.has_o2_mask = false

    self.has_zoomerator = false
    self.has_raritanium = false
    self.has_codebot = false
    self.has_premium_nanotech = false
    self.has_ultra_nanotech = false

    self.got_novalis_mayor = false
    self.got_oltanis_infobot = false
    self.got_oltanis_PDA = false
    self.got_oltanis_morph = false
    
    self.level_unlock_queue = {}
    self.item_unlock_queue = {}
    self.special_unlock_queue = {}

    self.received_gold_bolts = {}
    self.num_received_gold_bolts = 0
    self.used_gold_bolts = 0
    self.gold_bolt_pack_size = 1
    
    self.totalBolts = 0
    self.boltMultiplier = 1
    self.boltPackSize = 0
    self.num_used_bolt_packs = 0

    self.progressive_weapons = 0
    
    self.metal_detector_multiplier = 50
    
    self.slot_data = nil
    self.unlock_count = {}
    
    self.using_outdated_AP = false
    
    self.button = Button(self:GetLevelByName("Veldin2"), 415)
end

function RandoUniverse:Connect()
    if self.lobby.port == "" then
        self.host = self.lobby.address
    else
        self.host = self.lobby.address .. ":" .. self.lobby.port
    end

    local uuid = "ee4ff193-f687-45f4-806d-c7ad6778c743"
    
    self.ap_client = APClient(self, game_name, items_handling, uuid, self.host, self.lobby.slot, self.lobby.ap_password)
    self.ap_client_initialized = true
end

function RandoUniverse:DistributeGiveItem(item_id)
    table.insert(self.item_unlock_queue, item_id)
    if equip == nil then
        equip = false
    end
    
    item_name = Item.GetById(item_id).name
        if item_id == Item.GetByName("Hoverboard").id then
            self.has_hoverboard = true
        elseif item_id == Item.GetByName("O2 Mask").id then
            self.has_o2_mask = true
        end
    for _, player in ipairs(self:LuaEntity():FindChildren("Player")) do
        if player.fullySpawnedIn then
            player:ToastMessage("You received the \x0c" .. item_name .. "\x08")
            player:GiveItem(item_id, IsGameItemStartingItem(item_id))
            FixPlanetsForPlayer(self, player)
        end

        if player.gameState == 6 then
            table.insert(player.item_unlock_queue, item_id)
            player.receivedItemsWhileLoading = true
        end
    end

    if Item.GetById(item_id).isWeapon and item_id ~= 0x0e and item_id ~= 0x12 and item_id ~= 0x09 and item_id ~= 0x15 then -- is weapon that uses ammo
        if self.gotten_first_ammo_weapon == false then -- empty vendor protection
            self.gotten_first_ammo_weapon = true
            self.buyable_ammo = {}
        end
        if #self.buyable_ammo == 0 then
            table.insert(self.buyable_ammo, item_id)
            self:DistributeVendorContents()
        else
            for k, v in ipairs(self.buyable_ammo) do
                if v == item_id then break end -- if item already in list, do nothing
                if k == #self.buyable_ammo then -- if we have just checked the last item in the list and reached here, insert new item
                    table.insert(self.buyable_ammo, item_id)
                    self:DistributeVendorContents()
                    break
                end
            end
        end
    end
end

function RandoUniverse:DistributeUnlockSpecial(special_address)
    table.insert(self.special_unlock_queue, special_address)
    
    local toastMessage = ""
    
    if special_address == Player.offset.has_zoomerator then
        self.has_zoomerator = true
        toastMessage = "You received the \x0cZoomerator\x08"
    elseif special_address == Player.offset.has_raritanium then
        self.has_raritanium = true
        toastMessage = "You received the \x0cRaritanium\x08"
    elseif special_address == Player.offset.has_codebot then
        self.has_codebot = true
        toastMessage = "You received the \x0cCodebot\x08"
    elseif special_address == Player.offset.has_premium_nanotech then
        self.has_premium_nanotech = true
        toastMessage = "You received the \x0cPremium Nanotech\x08"
    elseif special_address == Player.offset.has_ultra_nanotech then
        self.has_ultra_nanotech = true
        toastMessage = "You received the \x0cUltra Nanotech\x08"
    end
    for _, player in ipairs(self:LuaEntity():FindChildren("Player")) do
        if player.fullySpawnedIn then
            player:ToastMessage(toastMessage)
            player:SetAddressValue(special_address, 1, 1)
            player:UpdateHPAmount()
        end

        if player.gameState == 6 then
            table.insert(player.special_unlock_queue, special_address)
            player.receivedItemsWhileLoading = true
        end
    end
end

function RandoUniverse:DistributeUnlockPlanet(planet_id)
    table.insert(self.level_unlock_queue, planet_id)
    self:AddPlanetVendorItem(planet_id)
    planet_name = self:GetLevelByGameID(planet_id):GetName()
    for _, player in ipairs(self:LuaEntity():FindChildren("Player")) do
        if player.fullySpawnedIn then
            player:ToastMessage("Infobot for planet \x0c" .. planet_name .. "\x08 received.")
            player:UnlockLevel(planet_id)
        end
        if player.gameState == 6 then
            table.insert(player.level_unlock_queue, planet_id)
            player.receivedItemsWhileLoading = true
        end
    end
end

function RandoUniverse:DistributeSetBolts(bolts)
    for _, player in ipairs(self:LuaEntity():FindChildren("Player")) do
        player:SetBolts(bolts)
    end
end

function RandoUniverse:DistributeSetLevelFlags(_type, level, index, value)
    for _, player in ipairs(self:LuaEntity():FindChildren("Player")) do
        player:SetLevelFlags(_type, level, index, value)
    end
end

function RandoUniverse:GiveAPItemToPlayers(ap_item, ap_location)
    if ap_item == nil then
        return
    end
    print("RandoUniverse:GiveAPItemToPlayers. item: " .. tostring(ap_item))
    ap_item_type = GetAPItemType(ap_item)
    
    if ap_item_type == "item" then
        self:DistributeGiveItem(APItemToItem(ap_item))
    elseif ap_item_type == "special" then
        if self.progressive_weapons == 1 and not (ap_item == 48 or ap_item == 49 or ap_item == 50 or ap_item == 52 or ap_item == 53) then -- normal (give both base and gold)
            self:GiveAPItemToPlayers(APGoldWeaponToAPBaseWeapon(ap_item), ap_location)
        end
        self:DistributeUnlockSpecial(APItemToSpecial(ap_item))
    elseif ap_item_type == "planet" then
        self:DistributeUnlockPlanet(APItemToPlanet(ap_item))
    elseif ap_item_type == "gold bolt" then
        if self.received_gold_bolts[ap_location] == nil then
            self.received_gold_bolts[ap_location] = ap_location
            self.num_received_gold_bolts = self.num_received_gold_bolts + self.gold_bolt_pack_size
        end
        self:DistributeGoldBoltValue()
    elseif ap_item_type == "bolt pack" then
        self.num_used_bolt_packs = self.num_used_bolt_packs + 1
        self:GiveBolts(self.boltPackSize, false)
    elseif ap_item_type == "progressive" then
        self:GiveAPItemToPlayers(ProgressiveAPItemToNormalAPItem(ap_item, self.slot_data, self.unlock_count), ap_location)
    else
        print("Unknown item: " .. tostring(ap_item))
    end
end

function RandoUniverse:OnPlayerJoin(player)
    print("player joined!")
    player:SetAddressValue(0xB00000, self.metal_detector_multiplier, 1) -- metal detector multiplier
    player:SetAddressValue(0xB00001, 1, 1) -- disable skid self delete
    player.level_unlock_queue = self.level_unlock_queue
    player.item_unlock_queue = self.item_unlock_queue
    player.special_unlock_queue = self.special_unlock_queue
    player.receivedItemsWhileLoading = true
end

function RandoUniverse:PlayerForceSyncItems(player)
    player.level_unlock_queue = self.level_unlock_queue
    player.item_unlock_queue = self.item_unlock_queue
    player.special_unlock_queue = self.special_unlock_queue
end

function RandoUniverse:OnPlayerGetItem(player, item_id)
    if item_id == 10 then -- bomb glove
        if self.using_outdated_AP then
            player:GiveItem(10)
        end
        return
    end
    location_id = ItemToLocation(item_id)
    self:OnPlayerGetLocation(player, location_id)
    self:NotifyPlayersLocationCollected(location_id, player)
end

function RandoUniverse:OnPlayerGetPlanet(player, planet_id)
    print("OnPlayerGetPlanet: " .. tostring(planet_id))
    --if (player.gameState == 6 or -- PlanetLoading
    --    not player.fullySpawnedIn) then
    --    print("Planet " .. planet_id .. " unlock_level called during planet loading. (ignoring)")
    --    
    --   player:UnlockLevel(planet_id)
    --   return
    --end
    location_id = PlanetToLocation(planet_id)
    self:OnPlayerGetLocation(player, location_id)
    self:NotifyPlayersLocationCollected(location_id, player)
end

function RandoUniverse:OnPlayerGetGoldBolt(player, planet, number)
    location_id = GoldBoltToLocation(planet, number)
    self:OnPlayerGetLocation(player, location_id)
    self:NotifyPlayersLocationCollected(location_id, player)
end

function RandoUniverse:OnPlayerGetLocation(player, location_id)
    LocationSync(self, player, location_id)
    self.ap_client:getLocation(location_id)
end

function RandoUniverse:NotifyPlayersLocationCollected(location_id, exclude_player)
    for _, _player in ipairs(self:LuaEntity():FindChildren("Player")) do
        if _player ~= exclude_player then
            LocationSync(self, _player, location_id)
        end
    end
end

function RandoUniverse:AddPlanetVendorItem(planet_id)
    item_id = GetPlanetVendorItem(planet_id)
    if item_id ~= nil then
        for _, v in ipairs(self.buyable_weapons) do if v == item_id then return end end -- end early if item already in list
        for _, v in ipairs(self.already_bought_weapons) do if v == item_id then return end end -- end early if item location was already bought
        table.insert(self.buyable_weapons, item_id)
        self:DistributeVendorContents()
    end
    --if planet_id == 1 and not self.using_outdated_AP then -- Novalis, add gold weapon hints
    --    self.ap_client:SendHint(95)
    --    self.ap_client:SendHint(96)
    --    self.ap_client:SendHint(97)
    --    self.ap_client:SendHint(98)
    --    self.ap_client:SendHint(99)
    --    self.ap_client:SendHint(100)
    --    self.ap_client:SendHint(101)
    --    self.ap_client:SendHint(102)
    --    self.ap_client:SendHint(103)
    --    self.ap_client:SendHint(104)
    --end
end

function RandoUniverse:DistributeVendorContents()
    for _, _player in ipairs(self:LuaEntity():FindChildren("Player")) do
        _player:UpdateVendorContents()
    end
end

function RandoUniverse:DistributeGoldBoltValue()
    for _, player in ipairs(self:LuaEntity():FindChildren("Player")) do
        player.GoldBoltCountLabel:SetText("Gold Bolts: "..tostring(self.num_received_gold_bolts - self.used_gold_bolts))
    end
end

function RandoUniverse:RemoveVendorItem(item_id)
    for k,v in ipairs(self.buyable_weapons) do
        if v == item_id then table.remove(self.buyable_weapons, k) break end
    end
    table.insert(self.already_bought_weapons, item_id)
    self:DistributeVendorContents()
end

function RandoUniverse:GiveBolts(boltDiff, enableMultiply)
    if enableMultiply == nil then
        enableMultiply = true
    end
    if enableMultiply and boltDiff > 0 then
        boltDiff = boltDiff * self.boltMultiplier
    end
    self.totalBolts = self.totalBolts + boltDiff
    self:DistributeSetBolts(self.totalBolts)
    local pureBolts = self.totalBolts - (self.num_used_bolt_packs * self.boltPackSize)
    self.ap_client:SetBolts(pureBolts)
    --print(string.format("new total bolt count: %d", self.totalBolts))
end

function RandoUniverse:APMessageReceived(msg)
    for _, player in ipairs(self:LuaEntity():FindChildren("Player")) do
        player:ToastMessage(msg, 300)
    end
end

function RandoUniverse:SendVendorHints()
    for _, item_id in ipairs(self.buyable_weapons) do
        location_id = ItemToLocation(item_id)
        self.ap_client:SendHint(location_id)
    end
end

function RandoUniverse:OnTick()
    if self.ap_client_initialized then
        self.ap_client:poll()
    end
end
