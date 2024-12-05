Fred = class("Fred", HybridMoby)

function Fred:initialize(level, uid)
    print("Initializing Fred")
    HybridMoby.initialize(self, level, uid)

    self.dead = false

    --self:MonitorAttribute(Moby.offset.state, 1)
    self:MonitorPVar(0x48, 4)
    
    self:MonitorAttribute(Moby.offset.pManipulator1, 4)
    self:MonitorAttribute(Moby.offset.pManipulator2, 4)
end

function Fred:OnAttributeChange(player, offset, oldValue, newValue)
    if offset == Moby.offset.state then
        print("Fred state changed from " .. oldValue .. " to " .. newValue)
    end
    
    if offset == Moby.offset.pManipulator1 then
        print("Fred pManipulator1 changed from " .. oldValue .. " to " .. newValue)
    end
    
    if offset == Moby.offset.pManipulator2 then
        print("Fred pManipulator2 changed from " .. oldValue .. " to " .. newValue)
    end
end

function Fred:OnPVarChange(player, offset, oldValue, newValue)
    if offset == 0x48 then
        player:ChangeMobyPVar(self.UID, 0x48, 4, 2)
        
        print("Fred PVar changed from " .. oldValue .. " to " .. newValue)
    end
end
