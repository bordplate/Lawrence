require 'Main.HASPlayer'

HASUniverse = class("HASUniverse", Universe)

LEVEL_POOL = {
    "Novalis",
    "Aridia",
    "Kerwan",
    "Eudora",
    "BlargStation",
    "Rilgar",
    "Umbris",
    "Batalia",
    "Gaspar",
    "Orxon",
    "Pokitaru",
    "Hoven",
    "GemlikStation",
    "Quartu",
    "KaleboIII",
    "DreksFleet",
    "Veldin2",
}

HAS_COUNTDOWN_TIME = 80 * 60

function HASUniverse:initialize(lobby)
    Universe.initialize(self)
    
    self.players = {}
    self.playerLabels = {}
    
    self.loaded = false
    
    self.lobby = lobby
    self.finishedCountdown = 0
    self.finished = false
    self.executedFinish = false

    self.started = false
    self.startTime = 0
    
    self.initialSeeker = nil
    
    -- Pick a random level
    local level = math.random(1, #LEVEL_POOL)
    self.selectedLevel = LEVEL_POOL[level]
end

function HASUniverse:OnPlayerJoin(player)
    player = player:Make(HASPlayer)
    player.seeker = false
    
    --player:LoadLevel(self.selectedLevel)
    
    print("Player " .. player:Username() .. " joined Hide & Seek")
    
    -- Find out if we already had the player and if they just disconnected and reconnected
    -- If they reconnected, we update their info and replace the old player
    for i, _player in ipairs(self.players) do
        if _player:Username() == player:Username() then
            print("Player " .. player:Username() .. " reconnected")
            
            player:LoadLevel(self.selectedLevel)

            if self.players[i].seeker then
                player:MakeSeeker("restored state")
            end
            
            player.startTime = self.players[i].startTime
            player.survivalTime = self.players[i].survivalTime
            
            player.started = true
            
            self.players[i] = player
            
            return
        end
    end

    if self.loaded then
        print(player:Username() .. " joined after game started")
        self.players[#self.players + 1] = player
        player:StartGame()
    end
end

function HASUniverse:CountHiders()
    local hiders = 0
    
    for i, player in ipairs(self.players) do
        if not player.seeker then
            hiders = hiders + 1
        end
    end
    
    return hiders
end

function HASUniverse:CountSeekers()
    local seekers = 0
    
    for i, player in ipairs(self.players) do
        if player.seeker then
            seekers = seekers + 1
        end
    end
    
    return seekers
end

function HASUniverse:GetIdlePlayers()
    local idlePlayers = {}
    
    for i, player in ipairs(self.players) do
        if not player.seeker and player.idleTimer > 120 * 60 then
            idlePlayers[#idlePlayers + 1] = player
            player:ToastMessage("Idle penalty: Seekers will be notified of your location")
        end
    end
    
    return idlePlayers
end

function HASUniverse:StartHAS()
    print("Starting Hide & Seek on " .. self.selectedLevel)
    
    for i, player in ipairs(self.players) do
        player:LoadLevel(self.selectedLevel)
    end
    
    self.finishedCountdown = 0
    self.finished = false
    
    -- Set this universe as primary so players join into this one instead of the lobby if they DC
    self:SetPrimary(true)
    
    self.players = self:FindChildren("Player")

    -- Select player number to be seeker
    math.randomseed(Game:Time())
    local seeker = math.random(1, #self.players)
    local seekerPlayer = self.players[seeker]
    
    self.initialSeeker = seekerPlayer
    
    seekerPlayer:MakeSeeker("randomly selected")
    print("Player " .. seekerPlayer:Username() .. " is seeker")
    
    print("Starting countdown")
    
    -- Make countdown label
    self.countdown = HAS_COUNTDOWN_TIME
    self.countdownLabel = Label:new("", 250, 330, 0xC0FFA888)
    self:AddLabel(self.countdownLabel)
    
    self.hiderCountLabel = Label:new("Hiders: " .. self:CountHiders(), 440, 50, 0xC0FFA888)
    self.seekerCountLabel = Label:new("Seekers: " .. self:CountSeekers(), 440, 70, 0xC0FFA888)
    
    self:AddLabel(self.hiderCountLabel)
    self:AddLabel(self.seekerCountLabel)
    
    self.loaded = true
end

function HASUniverse:ShowLeaderboard()
    -- Show a leaderboard in the middle of the screen ranking the survival times of players, with the longest time at the 
    --   top. The initial seeker's name is at the top of the screen. 
    
    -- Show seeker at the top
    self.initialSeekerLabel = Label:new("Initial seeker: " .. self.initialSeeker:Username(), 250, 70, 0xC0FFA888)
    self:AddLabel(self.initialSeekerLabel)
    
    -- Sort players by longest survivalTime
    table.sort(self.players, function(a, b)
        return a.survivalTime > b.survivalTime
    end)
    
    self.leaderboardLabels = {}
    
    for i, player in ipairs(self.players) do
        if player:Username() ~= self.initialSeeker:Username() then
            local label = Label:new(i .. ". " .. player:Username() .. ": " .. millisToTimeSeconds(player.survivalTime), 250, 110 + (i * 20), 0xC0FFA888)
            self.leaderboardLabels[#self.leaderboardLabels + 1] = label

            self:AddLabel(label)
        end
    end
end

function HASUniverse:HideLeaderboard()
    self:RemoveLabel(self.initialSeekerLabel)
    
    for i, label in ipairs(self.leaderboardLabels) do
        self:RemoveLabel(label)
    end
end

function HASUniverse:StartFinish(prompt)
    self.finishedCountdown = 15 * 60  -- 15 seconds at 60 FPS
    self.finished = true

    self.countdownLabel:SetText(prompt)
    self:AddLabel(self.countdownLabel)
    
    self:ShowLeaderboard()
    
    -- Stop players
    for i, player in ipairs(self.players) do
        player.started = false
    end
end

function HASUniverse:OnFinish()
    if self.executedFinish then
        return
    end
    
    print("Finished Hide & Seek")
    
    self.lobby:Reset()
    
    self:RemoveLabel(self.countdownLabel)
    self:RemoveLabel(self.hiderCountLabel)
    self:RemoveLabel(self.seekerCountLabel)
    
    self:HideLeaderboard()
    
    -- Put players back in lobby
    local players = self:FindChildren("Player")
    for i, player in ipairs(players) do
        player:Finished()
        self.lobby:ShowLobby(player)
    end
    
    self.executedFinish = true
end

-- OnTick runs 60 times per second
function HASUniverse:OnTick()
    if #self.players <= 0 then
        return
    end
    
    local hiders = 0
    local seekers = 0
    
    for i, player in ipairs(self.players) do
        if player.seeker and self.countdown > 0 then
            player.state = 114
            
            player.z = 5000
        end
        
        if player:Level():GetName() ~= self.selectedLevel then
            player:LoadLevel(self.selectedLevel)
        end
        
        if not player.seeker and not player:Disconnected() then
            hiders = hiders + 1
        end

        if player.seeker and not player:Disconnected() then
            seekers = seekers + 1
        end
    end
    
    -- If there are no more seekers, we make a random player the seeker
    if seekers <= 0 then
        local seeker = math.random(1, #self.players)
        local seekerPlayer = self.players[seeker]
        seekerPlayer:MakeSeeker("fallback")
        print("There were no more connected seekers. Player " .. seekerPlayer:Username() .. " is now seeker")
    end
    
    if self.started and not self.finished and (Game:Time() - self.startTime) > 30 * 60 * 1000 then  -- 30 minutes
        print("Game has been running for 30 minutes, ending it")
        
        self:StartFinish("Hiders win!")
    end

    if self.countdown <= 0 and hiders <= 0 then
        if not self.finished then
            print("No hiders left, game has finished")
            
            self:StartFinish("Seekers win!")
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
        
        self.hiderCountLabel:SetText("Hiders: " .. self:CountHiders())
        self.seekerCountLabel:SetText("Seekers: " .. self:CountSeekers())
        
        local idlePlayers = self:GetIdlePlayers()
        
        for i, player in ipairs(self.players) do
            player:SetIdlePlayers(idlePlayers)
        end
    end
    
    -- Update countdown only once every second
    if self.countdown > 0 and self.countdown % 60 == 0 then
        self.countdownLabel:SetText("" .. math.floor(self.countdown / 60))
    elseif self.countdown < 0 and not self.started then
        self.countdownLabel:SetText("GO!")
        
        self.started = true
        self.startTime = Game:Time()
        
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
