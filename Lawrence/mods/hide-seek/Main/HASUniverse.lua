require 'Main.HASPlayer'

HASUniverse = class("HASUniverse", Universe)

function HASUniverse:initialize()
    Universe.initialize(self)
    
    self.blocked_bolts = {}
    
    self.players = {}
    self.playerLabels = {}
    
    self.lobbyUniverse = nil
    
    self.finishedCountdown = 0
    self.finished = false
    
    self.selectedLevel = "Hoven"
end

function HASUniverse:OnPlayerJoin(player)
    player = player:Make(HASPlayer)
    
    player:LoadLevel(self.selectedLevel)
    
    -- Find out if we already had the player and if they just disconnected and reconnected
    -- If they reconnected, we update their info and replace the old player
    for i, _player in ipairs(self.players) do
        if _player:Username() == player:Username() then
            print("Player " .. player:Username() .. " reconnected")
            
            -- TODO: Restore items and put player back on the level they were on before disconnecting
            -- However, we can assume that if a player is a on a planet other than Veldin1, they are still in game
            --   so we should only restore items and level if they are on Veldin1.
            
            -- temporarily just put on veldin1
            player:LoadLevel(self.selectedLevel)
            
            self.players[i] = player
        end
    end

    for i, entry in ipairs(self.blocked_bolts) do
        print("Blocking " .. entry[1] .. " bolt: " .. entry[2])
        player:BlockGoldBolt(entry[1], entry[2])
    end
end

function HASUniverse:StartHAS(lobby)
    self.lobbyUniverse = lobby
    
    print("Starting Hide & Seek")
    
    self.finishedCountdown = 0
    self.finished = false
    
    -- Set this universe as primary so players join into this one instead of the lobby if they DC
    self:SetPrimary(true)
    
    self.players = self:FindChildren("Player")

    -- Select player number to be seeker
    local seeker = math.random(1, #self.players)
    local seekerPlayer = self.players[seeker]
    seekerPlayer:MakeSeeker()
    print("Player " .. seekerPlayer:Username() .. " is seeker")
    
    print("Starting countdown")
    
    -- Make countdown label
    self.countdown = 20 * 60  -- 60 seconds at 60 FPS
    self.countdownLabel = Label:new("", 250, 250, 0xC0FFA888)
    self:AddLabel(self.countdownLabel)
end

function HASUniverse:OnFinish()
    print("Finished Hide & Seek")
    
    self:RemoveLabel(self.countdownLabel)
    
    -- Put players back in lobby
    for i, player in ipairs(self.players) do
        player:Finished()
        self.lobbyUniverse:AddEntity(player)
    end
    
    self:Delete()
end

-- OnTick runs 60 times per second
function HASUniverse:OnTick()
    if #self.players <= 0 then
        return
    end
    
    local hiders = 0
    
    for i, player in ipairs(self.players) do
        if player.seeker and self.countdown > 0 then
            player.state = 114
            
            player.z = 5000
        end
        
        if player:Level():GetName() ~= self.selectedLevel then
            player:LoadLevel(self.selectedLevel)
        end
        
        if not player.seeker then
            hiders = hiders + 1
        end
    end

    if self.countdown <= 0 and hiders <= 0 then
        if not self.finished then
            print("No hiders left, game has finished")
            
            self.finishedCountdown = 15 * 60  -- 3 seconds at 60 FPS
            self.finished = true
            
            self.countdownLabel:SetText("Finished!")
            self:AddLabel(self.countdownLabel)
        end
    end

    if self.finished then
        if self.finishedCountdown <= 0 then
            self:OnFinish()
        end
    end
    
    -- Every second we go through the players to see if someone is stuck on the exact same XYZ coordinates. If they are, 
    --   we distribute them upwards across the Z axis to unstuck them.
    if self:Ticks() % 60 == 0 then
        for i, player in ipairs(self.players) do
            for j, _player in ipairs(self.players) do
                if player ~= _player then
                    if player.x == _player.x and player.y == _player.y and player.z == _player.z then
                        print("Player " .. player:Username() .. " is stuck, moving them")
                        player.y = player.y + 0.5
                    end
                end
            end
        end
    end
    
    -- Update countdown only once every second
    if self.countdown > 0 and self.countdown % 60 == 0 then
        self.countdownLabel:SetText("" .. math.floor(self.countdown / 60))
    elseif self.countdown < 0 and self.countdown > -60 then
        self.countdownLabel:SetText("GO!")
        
        -- Unfreeze players
        for i, player in ipairs(self.players) do
            player:StartGame()
            
            if player.seeker then
                player.z = -1000
                player:Unfreeze()
            end
        end
    end
    
    if self.countdown < -60 and self.finishedCountdown <= 0 then
        self:RemoveLabel(self.countdownLabel)
    else
        self.countdown = self.countdown - 1
        self.finishedCountdown = self.finishedCountdown - 1
    end
end 
