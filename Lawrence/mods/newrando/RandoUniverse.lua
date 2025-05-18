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
end

function RandoUniverse:DistributeGiveItem(item_id, equip)
    if equip == nil then
        equip = false
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
    player:GiveBolts(150000)
    player:GiveItem(Item.GetByName("Heli-pack").id)
    player:GiveItem(Item.GetByName("R.Y.N.O.").id)
    player:GiveItem(Item.GetByName("O2 Mask").id)
    player:GiveItem(Item.GetByName("Swingshot").id)
    player:GiveItem(Item.GetByName("Magneboots").id)
--     player:GiveItem(6)
--     player:GiveItem(4)
--     player:GiveItem(10)
--     player:GiveItem(2)
--     self:GiveAPItemToPlayers(48)
--     self:GiveAPItemToPlayers(49)
--     self:GiveAPItemToPlayers(50)
--     self:GiveAPItemToPlayers(52)
--     self:GiveAPItemToPlayers(53)
--     player:GiveItem(35)
--     player:GiveItem(26)
--     player:GiveItem(22)
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
    if not PlanetGotFromCorrectLocation(player:Level():GameID(), planet_id) then
       print("Planet " .. planet_id .. " unlock_level not called from infobot. (ignoring)")
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

function RandoUniverse:NovalisShipGotByPlayer(player)
    for _, _player in ipairs(self:LuaEntity():FindChildren("Player")) do
        _player:SetAddressValue(0x443941c0 + 0xbc, 0x2, 1) -- bridge half
        _player:SetAddressValue(0x443942c0 + 0xbc, 0x2, 1) -- bridge half

        _player:SetLevelFlags(1,1,0,{0xff}) -- ship flag
    end
end

function RandoUniverse:OnTick()
    if self.ap_client_initialized then
        self.ap_client:poll()
    end
end
