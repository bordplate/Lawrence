Clank = class("Clank", HybridMoby)

function Clank:initialize(level, uid)
    print("Initializing Clank")
    HybridMoby.initialize(self, level, uid)

    self.dead = false

    self:MonitorAttribute(Moby.offset.pManipulator1, 4)
    self:MonitorAttribute(Moby.offset.pManipulator2, 4)
end

function Clank:OnAttributeChange(player, offset, oldValue, newValue)
    if offset == Moby.offset.state then
        print("Clank state changed from " .. oldValue .. " to " .. newValue)
    end

    if offset == Moby.offset.pManipulator1 then
        print("Clank pManipulator1 changed from " .. oldValue .. " to " .. newValue)
    end

    if offset == Moby.offset.pManipulator2 then
        print("Clank pManipulator2 changed from " .. oldValue .. " to " .. newValue)
    end
end
