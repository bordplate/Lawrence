require 'Lobby.LobbyPlayer'
require 'Lobby.BarrierMoby'
require 'Lobby.StartMoby'

LobbyUniverse = class("LobbyUniverse", Universe)

function LobbyUniverse:initialize(mgbUniverse)
    Universe.initialize(self)
    
    self.mgbUniverse = mgbUniverse
    
    self.barrierMoby = self:GetLevelByName("Veldin1"):SpawnMoby(BarrierMoby)
    self.barrierMoby:SetPosition(146, 102, 33)
    
    self.readyMoby = self:GetLevelByName("Veldin1"):SpawnMoby(StartMoby)
    self.readyMoby:SetPosition(147, 100, 33)
end

-- When a new player joins this Universe. 
function LobbyUniverse:OnPlayerJoin(player)
    print("Player is joining Lobby")
    player:LoadLevel("Veldin1")
    player:Make(LobbyPlayer)
end

function LobbyUniverse:OnTick()
    local players = self:FindChildren("Player")
    
    if #players <= 0 then
        return
    end
    
    -- Go through all the players and check if they are ready. If all players are ready, start the game.
    local allReady = true
    for i, player in ipairs(players) do
        -- If player is not on Veldin1, put them there
        if player:Level():GetName() ~= "Veldin1" then
            player:LoadLevel("Veldin1")
        end
        
        if not player.ready then
            allReady = false
            break
        end
    end

    if allReady then
        print("All players ready.")
        
        -- Add players to mgbUniverse
        for i, player in ipairs(players) do
            player:LoadLevel("Veldin1")
            self.mgbUniverse:AddEntity(player)
        end
        
        self.mgbUniverse:StartMGB()
    end
end