require 'ReplaceNPCMobys'
require 'APClient'
require 'Locations'
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
    
    self.helga = self:GetLevelByName("Kerwan"):SpawnMoby(HelgaMoby)
    
    self.lobby = lobby
    
    self.ap_client = nil
    self.ap_client_initialized = false
end

function RandoUniverse:GiveAPItemToPlayers(ap_item)
    print("RandoUniverse:GiveAPItemToPlayers. item: " .. tostring(ap_item))
    ap_item_type = GetAPItemType(ap_item)
    for _, player in ipairs(self:LuaEntity():FindChildren("Player")) do
        if ap_item_type == "item" then
            player:GiveItem(APItemToItem(ap_item))
        elseif ap_item_type == "planet" then
            player:UnlockLevel(APItemToPlanet(ap_item))
        else
            APItemToGoldBolt(ap_item)
            player:GiveBolts(15000)
            -- do stuff for gold bolt (give money?)
        end
    end
end

function RandoUniverse:OnPlayerJoin(player)
    print("player joined!")
--     player:GiveBolts(150000)
--     player:GiveItem(6)
--     player:GiveItem(4)
--     player:GiveItem(10)
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
    -- stash location checked, and set flags for other players (and delete relevant npc's like helga)
    self.ap_client:getLocation(location_id)
end

function RandoUniverse:NotifyPlayersLocationCollected(location_id, exclude_player)
    for _, _player in ipairs(self:LuaEntity():FindChildren("Player")) do
        if _player ~= exclude_player then
            _player:NotifyLocationCollected(location_id)
        end
    end
end

function RandoUniverse:OnTick()
    if self.ap_client_initialized then
        self.ap_client:poll()
    end
end
