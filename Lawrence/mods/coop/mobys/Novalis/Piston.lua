Piston = class("Piston", HybridMoby)

function Piston:initialize(level, uid, lowZ, highZ)
    HybridMoby.initialize(self, level, uid)

    self:MonitorAttribute(Moby.offset.state, 1)
    self:MonitorAttribute(Moby.offset.position.z, 4, true)
    
    self.active = false
    
    self.lowZ = lowZ
    self.highZ = highZ
    
    self.zPos = lowZ
end

function Piston:SetZPos(pos)
    self:ChangeAttribute(Moby.offset.position.z, 4, pos, true)
end

function Piston:OnAttributeChange(player, offset, oldValue, newValue)
    if offset == Moby.offset.state then
        print("Piston state from " .. oldValue .. " changed to " .. newValue .. " by " .. player:Username())
        --player:ChangeMobyAttribute(self.UID, Moby.offset.pUpdate, 4, 0)
        self:SetZPos(self.zPos)
    end

    if offset == Moby.offset.position.z then
        print(player:Username() .. " is changing piston Z!")
        --player:ChangeMobyAttribute(self.UID, Moby.offset.pUpdate, 4, 0)
        self:SetZPos(self.zPos)
    end
end

function Piston:OnPVarChange(player, offset, oldValue, newValue)
    print("Piston pvar from " .. oldValue .. " changed to " .. newValue .. " by " .. player:Username())
end

function Piston:OnTick()
    if self.active and self.zPos < self.highZ then
        self.zPos = self.zPos + 0.075
        self:SetZPos(self.zPos)
        self:ChangeAttribute(0xbc, 1, 2)
    end
    
    if not self.active and self.zPos > self.lowZ then
        self.zPos = self.zPos - 0.075
        self:SetZPos(self.zPos)
        self:ChangeAttribute(0xbc, 1, 2)
    end
end
