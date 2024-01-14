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
    
    self:Reset()
end

-- When a new player joins this Universe. 
function LobbyUniverse:OnPlayerJoin(player)
    print("Player is joining Lobby")
    player = player:Make(LobbyPlayer)
    player:LoadLevel("KaleboIII")
    
    player.ready = false
end

function LobbyUniverse:Reset()
    self.startCountdown = 0
    self.shouldStart = false
    self.shouldCountdown = false
end

function LobbyUniverse:OnTick()
    local players = self:FindChildren("Player")
    
    if #players <= 0 then
        return
    end
    
    -- Go through all the players and check if they are ready. If all players are ready, start the game.
    local numReady = 0
    for i, player in ipairs(players) do
        -- If player is not on KaleboIII, put them there
        if player:Level():GetName() ~= "KaleboIII" then
            player:LoadLevel("KaleboIII")
        end
        
        if player.ready then
            numReady = numReady + 1
        end
    end

    if numReady == #players then
        self.startCountdown = 0
    end

    if numReady > 1 and not self.shouldStart then
        self.shouldStart = true
        self.startCountdown = 60 * 60 -- 60 seconds
    end
    
    self.startCountdown = self.startCountdown - 1

    if self.startCountdown > 0 and self.startCountdown < 30 * 60 then
        for i, player in ipairs(players) do
            player:ToastMessage("Game starting in " .. math.floor(self.startCountdown / 60) .. " seconds")
        end
    end

    if self.startCountdown < 0 and self.shouldStart then
        print("All players ready.")

        local universe = HASUniverse:new()
        
        -- Add players to mgbUniverse
        for i, player in ipairs(players) do
            --player:LoadLevel("KaleboIII")
            universe:AddEntity(player)
        end
        
        universe:StartHAS(self)
        
        self.shouldStart = false
    end
end