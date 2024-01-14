require 'Lobby.LobbyPlayer'
require 'Lobby.StartMoby'
require 'Main.HASUniverse'

LobbyUniverse = class("LobbyUniverse", Universe)

function LobbyUniverse:initialize()
    Universe.initialize(self)
    
    --self.barrierMoby = self:GetLevelByName("Veldin1"):SpawnMoby(BarrierMoby)
    --self.barrierMoby:SetPosition(146, 102, 33)
    
    self.readyMoby = self:GetLevelByName("KaleboIII"):SpawnMoby(StartMoby)
    self.readyMoby:SetPosition(304, 289, 119)
end

-- When a new player joins this Universe. 
function LobbyUniverse:OnPlayerJoin(player)
    print("Player is joining Lobby")
    player = player:Make(LobbyPlayer)
    player:LoadLevel("KaleboIII")
    
    player.ready = false
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
        if player:Level():GetName() ~= "KaleboIII" then
            player:LoadLevel("KaleboIII")
        end
        
        if not player.ready then
            allReady = false
            break
        end
    end
    
    if #players < 2 then
        allReady = false
    end

    if allReady then
        print("All players ready.")

        local universe = HASUniverse:new()
        
        -- Add players to mgbUniverse
        for i, player in ipairs(players) do
            --player:LoadLevel("KaleboIII")
            universe:AddEntity(player)
        end
        
        universe:StartHAS(self)
    end
end