require 'Race.Checkpoint'

RacePlayer = class('RacePlayer', Player)

DEFAULT_ITEMS = {
    2, 3, 4, 6,  28, 29, 12, 32, 11
}

function RacePlayer:Made()
    self.course = nil
    self.checkpoint = 0
    self.lap = 1
    self.checkpointMoby = nil
    self.race = nil
    
    self.placement = 1
    self.finished = false
    
    self.inactiveSince = Game:Time()
    self.lastPos = {x=0, y=0, z=0}

    self.placementLabel = Label:new("", 0, 370, 0xC0FFA888)
    self.checkpointLabel = Label:new(self.checkpoint .. "/?", 0, 390, 0xC0FFA888)
    
    self.finishTimeLabel = Label:new("", 420, 25, 0xC000FF00)
    self:AddLabel(self.finishTimeLabel)
    
    --self.rotZLabel = Label:new("rotZ: " .. self.rotZ, 400, 340, 0xC0FFA888)
    --self.coordsXLabel = Label:new("x: " .. self.rotZ, 400, 360, 0xC0FFA888)
    --self.coordsYLabel = Label:new("y: " .. self.y, 400, 380, 0xC0FFA888)
    --self.coordsZLabel = Label:new("z: " .. self.z, 400, 400, 0xC0FFA888)
    --self:AddLabel(self.rotZLabel)
    --self:AddLabel(self.coordsXLabel)
    --self:AddLabel(self.coordsYLabel)
    --self:AddLabel(self.coordsZLabel)
    
    -- Give all items
    for i, item in ipairs(DEFAULT_ITEMS) do
        self:GiveItem(item)
    end
    
    self.startTime = Game:Time()
    
    self.voting = false
    self.voteOptions = {}
    self.voteCallback = nil
    self.voteLabels = {}
    self.voted = -1
    self.voteTitleLabel = nil
end

local VOTE_BUTTONS = {
    "\x12", "\x11", "\x10", "\x13"
}

function RacePlayer:Reset()
    self.course = nil
    self.checkpoint = 0
    self.lap = 1
    self.checkpointMoby = nil

    self.placement = 1
    self.finished = false

    self.startTime = Game:Time()

    self.voting = false
    self.voteOptions = {}
    self.voteCallback = nil
    self.voteLabels = {}
    self.voted = -1
    self.voteTitleLabel = nil
end

function RacePlayer:StartVote(options, title, callback)
    self.voteOptions = options
    
    self:LockMovement()
    
    self.voted = -1

    self.voteTitleLabel = Label:new(title .. ":", 50, 310, 0xC0FFA888)
    self:AddLabel(self.voteTitleLabel)
    
    for i, option in ipairs(options) do
        local label = Label:new(VOTE_BUTTONS[i] .. " " .. option, 50, 330 + (i-1) * 20, 0xC0FFA888)
        self.voteLabels[#self.voteLabels+1] = label
        
        self:AddLabel(label)
    end
    
    self.voteCallback = callback
    
    self.voting = true
end


function RacePlayer:OnControllerInputTapped(input)
    if not self.voting or self.voted > 0 then
        return
    end

    -- Triangle input
    if input & 0x10 ~= 0 then
        self.voted = 1
    end

    -- Circle
    if input & 0x20 ~= 0 then
        self.voted = 2
    end

    -- Cross
    if input & 0x40 ~= 0 and #self.voteOptions > 2 then
        self.voted = 3
    end

    -- Square
    if input & 0x80 ~= 0 and #self.voteOptions > 3 then
        self.voted = 4
    end

    if self.voted > 0 then
        self.voteCallback(self.voted)
        self.voteLabels[self.voted]:SetColor(0xc000ff00)
    end
end

function RacePlayer:EndVote()
    for i, label in ipairs(self.voteLabels) do
        self:RemoveLabel(label)
    end
    
    self:RemoveLabel(self.voteTitleLabel)
    
    self.voting = false
    self.state = 0
end

function RacePlayer:StartRace()
    self:AddLabel(self.checkpointLabel)
    self:AddLabel(self.placementLabel)
end

function RacePlayer:EndRace()
    self:RemoveLabel(self.checkpointLabel)
    self:RemoveLabel(self.placementLabel)
end

function RacePlayer:SetPlacementText()
    local text = self.placement .. ""
    if self.placement == 1 then
        text = text .. "st"
    elseif self.placement == 2 then
        text = text .. "nd"
    elseif self.placement == 3 then
        text = text .. "rd"
    else
        text = text .. "th"
    end
    
    self.placementLabel:SetText(text .. " place")
end

function RacePlayer:SetCourse(course)
    self:Reset()
    
    self.course = course
    self.finished = false
    self.checkpoint = 0
    self:LoadLevel(course.planet)
    self:SetRespawn(course.start.x, course.start.y, course.start.z, course.start.rotation)
    self.checkpointLabel:SetText(self.checkpoint .. "/" .. #self.course.checkpoints)
    
    self.finishTimeLabel:SetText("")
end

function RacePlayer:SpawnCheckpoint()
    local check = self.course.checkpoints[self.checkpoint+1]

    self.checkpointMoby = self:SpawnInstanced(Checkpoint)
    self.checkpointMoby:SetPosition(check.x, check.y, check.z)
end

function RacePlayer:Finished()
    self.finishTimeLabel:SetText(self.race.timerLabel:Text())
end

function RacePlayer:OnCollision(moby)
    if self.race.mode ~= 3 then
        return
    end
    
    if moby ~= nil and moby:GUID() == self.checkpointMoby:GUID() then
        print("Player hit checkpoint")
        local checkpoint = self.course.checkpoints[self.checkpoint+1]
        self:SetRespawn(checkpoint.x, checkpoint.y, checkpoint.z, checkpoint.rotation)

        self.checkpoint = self.checkpoint + 1
        self.checkpointLabel:SetText(self.checkpoint .. "/" .. #self.course.checkpoints)
        
        if self.checkpoint < #self.course.checkpoints then
            self:ToastMessage("Checkpoint", 0xbc)
            
            self:SpawnCheckpoint()
        else
            self:ToastMessage(self.placementLabel:Text(), 0xbc)
        end

        moby:Delete()
    end
end

function RacePlayer:OnRespawned()
    if self.checkpointMoby == nil then
        self:SpawnCheckpoint()
    end
    
    for i, moby in ipairs(self.course.delete) do
        self:DeleteAllChildrenWithOClass(moby)
    end

    for i, uid in ipairs(self.course.deleteUIDs) do
        self:DeleteAllChildrenWithUID(uid)
    end
end

function RacePlayer:DistanceToNextCheckpoint()
    if self.checkpointMoby == nil then
        return 0
    end
    
    return self:DistanceTo(self.checkpointMoby)
end

function RacePlayer:OnTick()
    --self.rotZLabel:SetText("rotZ: " .. self.rotZ)
    --self.coordsXLabel:SetText("x: " .. self.x)
    --self.coordsYLabel:SetText("y: " .. self.y)
    --self.coordsZLabel:SetText("z: " .. self.z)
    
    if not self.finished and self.race.mode == 3 and (
            self.lastPos.x ~= self.x or self.lastPos.y ~= self.y or self.lastPos.z ~= self.z
    ) then
        self.inactiveSince = Game:Time()
    end
    
    self.lastPos.x = self.x
    self.lastPos.y = self.y
    self.lastPos.z = self.z

    if self.voting then
        self.state = 114
    end

    -- Unfreeze erroneously frozen players
    if self.race.mode ~= 3 and self.state == 114 then
        self.state = 0
    end
end 