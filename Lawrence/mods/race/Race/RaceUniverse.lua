require 'Race.Checkpoint'
require 'Race.RacePlayer'
require 'Race.Course'

RaceUniverse = class("RaceUniverse", Universe)

COURSE_FILES = {"gemlik.json", "eudora_backwards.json", "poki.json", "rilgar.json", "orxon.json", "aridia.json"}

-- All of these units are in seconds
WAITING_TIMEOUT = 30
COUNTDOWN_SECONDS = 5
VOTING_TIME = 15
MAX_INACTIVE_TIME = 40
MAX_RACE_TIME = 8 * 60

RACE_MODE_WAITING = 1
RACE_MODE_COUNTDOWN = 2
RACE_MODE_RUNNING = 3
RACE_MODE_ENDING = 4

function RaceUniverse:initialize()
    Universe.initialize(self)

    self.maxPlayers = 8
    self.playerCount = 0
    
    self.racingPlayers = {}
    self.playersFinished = {}
    self.leaderboardLabels = {}
    
    self.allCourses = {}
    self.course = nil
    
    self.timerLabel = Label:new("00:00.000", 420, 10, 0xC0FFA888)
    self.waitingLabel = Label:new("Starting in ...", 250, 250, 0xC0FFA888)
    self.countdownLabel = Label:new("", 250, 250, 0xC0FFA888)

    self.mode = RACE_MODE_WAITING

    self.startTime = nil
    self.started = false

    self.waitingStartedTime = nil
    self.waitingTimeout = 0

    self.countdownEnd = 0
    
    self.voteCourses = {}
    self.playersVoted = 0
    self.voteEndTime = 0
    self.voteCountdownLabel = Label:new("", 220, 390, 0xC0FFA888)
    
    self:LoadCourses()
end

function RaceUniverse:LoadCourses()
    for i, file in ipairs(COURSE_FILES) do
        print("Loading: " .. file)
        self.allCourses[#self.allCourses+1] = Course:new(file)
    end
    
    self.course = pickUniqueItems(self.allCourses, 2)[1]
    self.planet = self.course.planet
end

--- When a new player joins this Universe. 
function RaceUniverse:OnPlayerJoin(player)
    print("Player is joining race")
    
    player = player:Make(RacePlayer)
    player.race = self
    
    player:SetCourse(self.course)

    -- If waiting time is less than 20, set it to 20 to allow the new player more than enough time to load the level.
    if self.mode == RACE_MODE_WAITING then
        local timeLeft = self.waitingTimeout - Game:Time()

        if timeLeft < 20 * 1000 then
            self.waitingTimeout = Game:Time() + 20 * 1000
        end
    end
end

--- Starts waiting for players to connect and stuff.
function RaceUniverse:StartWaiting()
    print("Started waiting for new race on " .. self.course.name)

    self.planet = self.course.planet
    
    self.mode = RACE_MODE_WAITING
    self.waitingStartedTime = nil
    self.playersFinished = {}
    self.racingPlayers = {}
    
    local players = self:FindChildren("Player")
    for i, player in ipairs(players) do
        player:SetCourse(self.course)
    end
end

--- Sets up and starts the race countdown. 
function RaceUniverse:StartRaceCountdown()
    self.racingPlayers = self:FindChildren("Player")
    
    print("Starting race countdown for " .. #self.racingPlayers .. " players...")
    
    -- Place the players on the starting line and freeze them
    for i, player in ipairs(self.racingPlayers) do
        player.x = self.course.start.x
        player.y = self.course.start.y + (i-1) * 1
        player.z = self.course.start.z
        --player.rotZ = (math.pi / 180) * self.course.start.rotation -- Degrees to radians conversion
        player.rotZ = self.course.start.rotation
        
        -- Frozen state, can't move or menu
        --player:LockMovement()
        
        local leaderboardLabel = Label:new(i .. ". " .. player:Username(), 0, 10 + (i-1) * 15, 0xC0FFA888)
        self:AddLabel(leaderboardLabel)
        
        self.leaderboardLabels[#self.leaderboardLabels+1] = leaderboardLabel
    end
    
    self.mode = RACE_MODE_COUNTDOWN
    self.countdownEnd = Game:Time() + COUNTDOWN_SECONDS * 1000
    
    self:AddLabel(self.countdownLabel)
end

--- Sets up and starts the race
-- Expected to be run after the countdown has finished.
function RaceUniverse:StartRace()
    self:AddLabel(self.timerLabel)

    for i, player in ipairs(self.racingPlayers) do
        player:UnlockMovement()
        player.inactiveSince = Game:Time()
        player:StartRace()
        player:SetSpeed(0)
    end

    self.startTime = Game:Time()

    self.mode = RACE_MODE_RUNNING
    
    print("Starting race with " .. #self.racingPlayers .. " players!")
end

function RaceUniverse:EndRace()
    print("Race ended!")
    
    self.voteCourses = {}
    self.voteEndTime = Game:Time() + VOTING_TIME * 1000
    
    self:RemoveLabel(self.timerLabel)
    
    self:AddLabel(self.voteCountdownLabel)

    local courseNames = {}

    print("Next courses to vote for:")
    for i, course in ipairs(pickUniqueItems(self.allCourses, 4)) do
        course.votes = 0
        self.voteCourses[#self.voteCourses+1] = course
        courseNames[#courseNames+1] = course.name
        print("> " .. course.name)
    end

    for i, player in ipairs(self.racingPlayers) do
        player:EndRace()
        local label =  self.leaderboardLabels[i]
        label:SetPosition(250, 120 + (i-1) * 15)
        
        label:SetText(i .. ". " .. player:Username())
        
        -- Start vote for next map
        player:StartVote(courseNames, "Vote for next course", function(option)
            print(player:Username() .. " voted for " .. self.voteCourses[option].name)
            self.voteCourses[option].votes = self.voteCourses[option].votes + 1
        end)
    end
    
    self.mode = RACE_MODE_ENDING
end

--- Called each tick while we're waiting to start the race.
function RaceUniverse:WaitingToStartTick()
    local players = self:FindChildren("Player")
    if self.waitingStartedTime == nil and #players > 0 then
        self.waitingStartedTime = Game:Time()
        self.waitingTimeout = Game:Time() + WAITING_TIMEOUT * 1000
        
        self:AddLabel(self.waitingLabel)
        
        print("Started waiting timeout.")
    end

    if self.waitingStartedTime == nil then
        return
    end

    -- We've hit the waiting timeout, we should start the race
    if self.waitingTimeout <= Game:Time() then
        self:RemoveLabel(self.waitingLabel)
        self:StartRaceCountdown()
    end
    
    local timeLeft = self.waitingTimeout - Game:Time()
    
    self.waitingLabel:SetText(string.format("Starting in %02d...", math.floor(timeLeft / 1000)))
end

-- Caled each tick while we're counting down the race start.
function RaceUniverse:CountdownTick()
    local timeLeft = self.countdownEnd - Game:Time()

    self.countdownLabel:SetText(string.format("%d!", math.floor(timeLeft / 1000) + 1))

    for i, player in ipairs(self.racingPlayers) do
        player:LockMovement()
        player:SetSpeed(0)
    end
    
    if timeLeft <= 0 then
        self:RemoveLabel(self.countdownLabel)
        self:StartRace()
    end
end

-- Called each tick while the race is on.
function RaceUniverse:RaceRunningTick()
    self.timerLabel:SetText(millisToTime(Game:Time() - self.startTime))
    
    ::before_loop_racing_players::
    for i, player in ipairs(self.racingPlayers) do
        -- Check for how long this player hasn't moved and kick if inactive
        local inactiveSeconds = (Game:Time() - player.inactiveSince) / 1000
        if inactiveSeconds > MAX_INACTIVE_TIME then
            print("Kicking inactive player: " .. player:Username())
            
            player:EndRace()
            player:Disconnect()
        end
        
        -- Check if player has disconnected, and boot them from race
        if player:Disconnected() then
            -- Remove them from the racing players list and restart the loop
            table.remove(self.racingPlayers, i)
            
            -- Remove player from leaderboards and clean up the label
            -- We don't bother finding the right label corresponding to the disconnected player to remove, 
            --   we just remove the bottom label, the order should fix itself.
            self:RemoveLabel(self.leaderboardLabels[#self.leaderboardLabels])
            table.remove(self.leaderboardLabels, #self.leaderboardLabels)
            
            -- Restart iterating through the players
            goto before_loop_racing_players
        end
        
        -- Check that player isn't still frozen during the race.
        if player.state == 114 then
            print("Player inadvertently frozen. Unfreezing")
            player.state = 0
        end
        
        if not player.finished and player.checkpoint >= #self.course.checkpoints then
            self.playersFinished[#self.playersFinished+1] = player
            player.finished = true
            player:Finished()
        end
    end
    
    -- If the race has exceeded the max time for races, end the race.
    local timeRan = Game:Time() - self.startTime
    if (timeRan / 1000) > MAX_RACE_TIME then
        self:EndRace()
    end
    
    -- If everyone has finished the race, end race
    if #self.playersFinished >= #self.racingPlayers then
        self:EndRace()
    end

    if Game:Ticks() % 30 ~= 0 then
        return
    end

    local sortedPlayers = {}

    -- Go through all the players and sort them by their race placement
    for i, player in ipairs(self.racingPlayers) do
        if #sortedPlayers > 0 then
            for j, sortedPlayer in ipairs(sortedPlayers) do
                if player.checkpoint > sortedPlayer.checkpoint or
                    (
                        player.checkpoint == sortedPlayer.checkpoint and
                        player:DistanceToNextCheckpoint() < sortedPlayer:DistanceToNextCheckpoint()
                    )
                then
                    table.insert(sortedPlayers, j, player)
                    goto found_placement
                end
            end
        end

        sortedPlayers[#sortedPlayers+1] = player

        ::found_placement::
    end

    for i, player in ipairs(sortedPlayers) do
        player.placement = i
        player:SetPlacementText()

        self.leaderboardLabels[i]:SetText(i .. ". " .. player:Username())
    end
end

function RaceUniverse:EndingTick()
    local timeLeft = self.voteEndTime - Game:Time()

    self.voteCountdownLabel:SetText(string.format("%d", math.floor(timeLeft / 1000) + 1))

    for i, player in ipairs(self.racingPlayers) do
        player:LockMovement()
    end

    if timeLeft <= 0 then
        self:RemoveLabel(self.voteCountdownLabel)

        for i, player in ipairs(self.racingPlayers) do
            if player.voting then
                player:EndVote()
            end
        end
        
        local votedCourse = nil
        for i, course in ipairs(self.voteCourses) do
            if votedCourse == nil or votedCourse.votes < course.votes then
                votedCourse = course
            end
        end
        
        self.course = votedCourse

        for i, label in ipairs(self.leaderboardLabels) do
            self:RemoveLabel(label)
        end
        
        self.leaderboardLabels = {}
        
        self:StartWaiting()
    end
end

function RaceUniverse:OnTick()
    if self.mode == RACE_MODE_WAITING then
        self:WaitingToStartTick()
    elseif self.mode == RACE_MODE_COUNTDOWN then
        self:CountdownTick()
    elseif self.mode == RACE_MODE_RUNNING then
        self:RaceRunningTick()
    elseif self.mode == RACE_MODE_ENDING then
        self:EndingTick()
    end
end