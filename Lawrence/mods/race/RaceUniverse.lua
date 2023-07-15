require 'Checkpoint'
require 'RacePlayer'
require 'Course'

RaceUniverse = class("RaceUniverse", Universe)

function RaceUniverse:initialize()
    Universe.initialize(self)

    self.maxPlayers = 8
    self.playerCount = 0
    
    self.course = Course:new()
    
    self.raceLabel = Label:new("Race", 250, 250, 0xff0000ff)

    self.timerLabel = Label:new("00:00.000", 320, 10, 0xC0FFA888)
    self:AddLabel(self.timerLabel)
    
    self.startTime = nil
    
    self.started = true
end

function LoadRace(raceName)
    
end

-- When a new player joins this Universe. 
function RaceUniverse:OnPlayerJoin(player)
    player:LoadLevel("Eudora")
    
    player = player:Make(RacePlayer)
    
    player:SetCourse(self.course)
    
    print("Player has joined race.")
end

function millisToTime(millis)
    local total_seconds = math.floor(millis / 1000)
    local minutes = math.floor(total_seconds / 60)
    local seconds = total_seconds - (minutes * 60)
    local milliseconds = millis - (total_seconds * 1000)
    return string.format("%02d:%02d.%03d", minutes, seconds, milliseconds)
end

function RaceUniverse:OnTick()
    if self.started then
        if self.startTime == nil then
            self.startTime = Game:Time()
        end
        
        self.timerLabel:SetText(millisToTime(Game:Time() - self.startTime))
    end
    
    if Game:Ticks() % 30 ~= 0 then
        return
    end
    
    local players = self:FindChildren("Player")
    
    local sortedPlayers = {}

    -- Go through all the players and sort them by their race placement
    for i, player in ipairs(players) do
        if #sortedPlayers > 0 then
            for j, sortedPlayer in ipairs(sortedPlayers) do
                if player.checkpoint > sortedPlayer.checkpoint or 
                        (player.checkpoint == sortedPlayer.checkpoint and player:DistanceToNextCheckpoint() < sortedPlayer:DistanceToNextCheckpoint()) then
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
    end
end