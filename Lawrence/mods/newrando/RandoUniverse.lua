require 'ReplaceNPCMobys'
require 'APClient'
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

function RandoUniverse:GiveItemToPlayers(item)
    print("RandoUniverse:GiveItemToPlayers. item: " .. item)
    for _, player in ipairs(self:LuaEntity():FindChildren("Player")) do
        if item > 100 then
            player:UnlockLevel(item - 100)
        else
            player:GiveItem(item, false)
        end
    end
end

function RandoUniverse:OnPlayerJoin(player)
    print("player joined!")
    if self.ap_client == nil then
        local uuid = "5"
        self.ap_client = APClient(self, game_name, items_handling, uuid, host, slot, password)
        self.ap_client_initialized = true
    else
        -- sync new player with the other
        print("AP already defined")
    end
end

function RandoUniverse:OnTick()
    if self.ap_client_initialized then
        self.ap_client:poll()
    end
end
