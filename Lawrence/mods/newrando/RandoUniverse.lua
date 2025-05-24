require 'ReplacementMobys.ReplacementMobys'
require 'APClient'
require 'Locations'
require 'LocationSyncing'
require 'Items'
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
    self.buyable_ammo = {} -- list weapons, +64 is performed to turn it into ammo
    self.already_bought_weapons = {}
end

function RandoUniverse:DistributeGiveItem(item_id, equip)
    if equip == nil then
        equip = false
    end

    if Item.GetById(item_id).isWeapon and item_id ~= 0x0e and item_id ~= 0x12 and item_id ~= 0x09 and item_id ~= 0x15 then -- is weapon that uses ammo
        for k, v in ipairs(self.buyable_ammo) do
            if v == item_id then break end -- if item already in list, do nothing
            if k == #self.buyable_ammo then -- if we have just checked the last item in the list and reached here, insert new item
                table.insert(self.buyable_ammo, item_id)
                self:DistributeVendorContents()
                break
            end
        end
    end
    
    for _, player in ipairs(self:LuaEntity():FindChildren("Player")) do
        if item_id == Item.GetByName("Hoverboard").id then
            player.has_hoverboard = true
        end
        
        if player.fullySpawnedIn then
            player:GiveItem(item_id, equip)
        else
            player.item_unlock_queue[#player.item_unlock_queue+1] = item_id
        end
    end
end

function RandoUniverse:DistributeUnlockSpecial(special_address)
    for _, player in ipairs(self:LuaEntity():FindChildren("Player")) do
        if special_address == Player.offset.has_zoomerator then
            player.has_zoomerator = true
        elseif special_address == Player.offset.has_raritanium then
            player.has_raritanium = true
        elseif special_address == Player.offset.has_codebot then
            player.has_codebot = true
        elseif special_address == Player.offset.has_premium_nanotech then
            player.has_premium_nanotech = true
        elseif special_address == Player.offset.has_ultra_nanotech then
            player.has_ultra_nanotech = true
        end
        
        if player.fullySpawnedIn then            
            player:SetAddressValue(special_address, 1, 1)
        else
            player.special_unlock_queue[#player.special_unlock_queue+1] = special_address
        end
    end
end

function RandoUniverse:DistributeUnlockPlanet(planet_id)
    for _, player in ipairs(self:LuaEntity():FindChildren("Player")) do
        if player.fullySpawnedIn then
            player:UnlockLevel(planet_id)
        else
            player.level_unlock_queue[#player.level_unlock_queue+1] = planet_id
        end
    end
end

function RandoUniverse:DistributeGiveBolts(bolts)
    for _, player in ipairs(self:LuaEntity():FindChildren("Player")) do
        player:GiveBolts(bolts)
    end
end

function RandoUniverse:DistributeSetLevelFlags(_type, level, index, value)
    for _, player in ipairs(self:LuaEntity():FindChildren("Player")) do
        player:SetLevelFlags(_type, level, index, value)
    end
end

function RandoUniverse:GiveAPItemToPlayers(ap_item)
    print("RandoUniverse:GiveAPItemToPlayers. item: " .. tostring(ap_item))
    ap_item_type = GetAPItemType(ap_item)
    
    if ap_item_type == "item" then
        self:DistributeGiveItem(APItemToItem(ap_item), true)
    elseif ap_item_type == "special" then
        self:DistributeUnlockSpecial(APItemToSpecial(ap_item))
    elseif ap_item_type == "planet" then
        self:DistributeUnlockPlanet(APItemToPlanet(ap_item))
    else
--         APItemToGoldBolt(ap_item)
        self:DistributeGiveBolts(15000)
    end
end

function RandoUniverse:OnPlayerJoin(player)
    print("player joined!")
    player:SetAddressValue(0xB00000, 50, 1)
    player:SetAddressValue(0xB00001, 1, 1)
    if self.ap_client == nil then
        local uuid = "5"
        self.ap_client = APClient(self, game_name, items_handling, uuid, host, slot, password)
        self.ap_client_initialized = true
    else
        -- sync new player with the other
        print("AP already defined")
    end
end

function RandoUniverse:OnPlayerGetItem(player, item_id)
    if item_id == 10 then -- bomb glove
        player:GiveItem(10)
        return
    end
    location_id = ItemToLocation(item_id)
    self:OnPlayerGetLocation(player, location_id)
    self:NotifyPlayersLocationCollected(location_id, player)
end

function RandoUniverse:OnPlayerGetPlanet(player, planet_id)
    print("OnPlayerGetPlanet: " .. tostring(planet_id))
    if player.gameState == 6 or -- PlanetLoading
        not player.fullySpawnedIn then 
        print("Planet " .. planet_id .. " unlock_level called during planet loading. (ignoring)")
       player:UnlockLevel(planet_id)
       return
    end
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
            _player:NotifyLocationCollected(location_id)
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
end

function RandoUniverse:DistributeVendorContents()
    for _, _player in ipairs(self:LuaEntity():FindChildren("Player")) do
        _player:UpdateVendorContents()
    end
end

function RandoUniverse:RemoveVendorItem(item_id)
    for k,v in ipairs(self.buyable_weapons) do
        if v == item_id then table.remove(self.buyable_weapons, k) break end
    end
    table.insert(self.already_bought_weapons, item_id)
    self:DistributeVendorContents()
end

function RandoUniverse:OnTick()
    if self.ap_client_initialized then
        self.ap_client:poll()
    end
end
