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
end

function LoadRace(raceName)
    
end

-- When a new player joins this Universe. 
function RaceUniverse:OnPlayerJoin(player)
    player:LoadLevel("Eudora")
    
    player = player:Make(RacePlayer)

    print("Whawdadawdt")
    
    player:SetCourse(self.course)
    
    print("Player has joined race.")
end

function RaceUniverse:OnTick()
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