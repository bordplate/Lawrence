require 'Checkpoint'
require 'NPCTest'

RacePlayer = class('RacePlayer', Player)

function RacePlayer:Made()
    self.course = nil
    self.checkpoint = 1
    self.lap = 1
    self.checkpointMoby = nil
    
    self.placement = 1

    self.placementLabel = Label:new("?st place", 150, 370, 0xC0FFA888)
    self.checkpointLabel = Label:new(self.checkpoint .. "/?", 150, 390, 0xC0FFA888)
    
    self.coordsXLabel = Label:new("x: " .. self.x, 400, 360, 0xC0FFA888)
    self.coordsYLabel = Label:new("y: " .. self.y, 400, 380, 0xC0FFA888)
    self.coordsZLabel = Label:new("z: " .. self.z, 400, 400, 0xC0FFA888)
    self:AddLabel(self.coordsXLabel)
    self:AddLabel(self.coordsYLabel)
    self:AddLabel(self.coordsZLabel)

    self:AddLabel(self.checkpointLabel)
    
    self:AddLabel(self.placementLabel)

    self.itemTimer = 0
    self.itemNext = 3
    
    self:GiveItem(2)
    
    self.timerLabel = Label:new("00:00.000", 320, 10, 0xC0FFA888)
    self:AddLabel(self.timerLabel)
    
    self.startTime = Game:Time()
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
    self.course = course
    self.checkpoint = 1
    self:SetRespawn(course.start.x, course.start.y, course.start.z, course.start.rotation)
    
    self:SpawnCheckpoint()
end

function RacePlayer:SpawnCheckpoint()
    print("Spawning checkpoint")
    
    local check = self.course.checkpoints[self.checkpoint]

    self.checkpointMoby = self:SpawnInstanced(Checkpoint)
    self.checkpointMoby:SetPosition(check.x, check.y, check.z)

    self.checkpointLabel:SetText(self.checkpoint .. "/" .. #self.course.checkpoints)
end

function RacePlayer:OnCollision(moby)
    if moby ~= nil and moby:GUID() == self.checkpointMoby:GUID() then
        print("Player hit checkpoint")
        local checkpoint = self.course.checkpoints[self.checkpoint]
        self:SetRespawn(checkpoint.x, checkpoint.y, checkpoint.z, checkpoint.rotation)

        if self.checkpoint < #self.course.checkpoints then
            self.checkpoint = self.checkpoint + 1
            
            self:SpawnCheckpoint()
        else
            local winLabel = Label:new("You win!", 200, 200, 0xC0FFA888)
            self:AddLabel(winLabel)
        end

        moby:Delete()
    end
end

function RacePlayer:OnRespawned()
    
end

function RacePlayer:DistanceToNextCheckpoint()
    if self.checkpointMoby == nil then
        return 0
    end
    
    return self:DistanceTo(self.checkpointMoby)
end

function millisToTime(millis)
    local total_seconds = math.floor(millis / 1000)
    local minutes = math.floor(total_seconds / 60)
    local seconds = total_seconds - (minutes * 60)
    local milliseconds = millis - (total_seconds * 1000)
    return string.format("%02d:%02d.%03d", minutes, seconds, milliseconds)
end

function RacePlayer:OnTick()
    self.timerLabel:SetText(millisToTime(Game:Time() - self.startTime))

    -- Hacky way to give all items due to a bug that locks up network communication when giving all items at the same time
    if (self.x ~= 0 and self.itemNext < 36) then
        self.itemTimer = self.itemTimer + 1
        if (self.itemTimer % 10) == 0 then
            self:GiveItem(self.itemNext)
            self.itemNext = self.itemNext + 1
        end
    end

    self.coordsXLabel:SetText("x: " .. self.x)
    self.coordsYLabel:SetText("y: " .. self.y)
    self.coordsZLabel:SetText("z: " .. self.z)
end 