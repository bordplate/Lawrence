Clank = class("Clank", HybridMoby)

function Clank:initialize(level, uid)
    HybridMoby.initialize(self, level, uid)

    self.dead = false

    self:MonitorAttribute(Moby.offset.pManipulator1, 4)
    self:MonitorAttribute(Moby.offset.pManipulator2, 4)
end

function Clank:OnAttributeChange(player, offset, oldValue, newValue)
    if offset == Moby.offset.state then
    end

    if offset == Moby.offset.pManipulator1 then
    end

    if offset == Moby.offset.pManipulator2 then
    end
end
