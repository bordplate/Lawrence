require 'Checkpoint'

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
    
    -- Give all items
    for i = 2, 35 do
        self:GiveItem(i)
    end
    
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
            
            self:ToastMessage("Checkpoint", 0xbc)
            
            self:SpawnCheckpoint()
        else
            self:ToastMessage("You win", 0xbc)
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

function RacePlayer:OnTick()
    --self.state = 114
    --self.state = 0

    self.coordsXLabel:SetText("x: " .. self.x)
    self.coordsYLabel:SetText("y: " .. self.y)
    self.coordsZLabel:SetText("z: " .. self.z)
end 