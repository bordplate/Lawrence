BridgePart = class("BridgePart", HybridMoby)

function BridgePart:initialize(level, uid)
    HybridMoby.initialize(self, level, uid)

    self:MonitorAttribute(Moby.offset.position.x, 4, true)
    self:MonitorAttribute(Moby.offset.position.y, 4, true)
    self:MonitorAttribute(Moby.offset.position.z, 4, true)
    self:MonitorAttribute(Moby.offset.rotation.x, 4, true)
    self:MonitorAttribute(Moby.offset.rotation.y, 4, true)
    self:MonitorAttribute(Moby.offset.rotation.z, 4, true)
end

function BridgePart:OnAttributeChange(player, offset, oldValue, newValue)
    if offset == Moby.offset.position.x then
        self:ChangeAttributeForOtherPlayers(player, Moby.offset.position.x, 4, newValue, true)
    end
    if offset == Moby.offset.position.y then
        self:ChangeAttributeForOtherPlayers(player, Moby.offset.position.y, 4, newValue, true)
    end
    if offset == Moby.offset.position.z then
        self:ChangeAttributeForOtherPlayers(player, Moby.offset.position.z, 4, newValue, true)
    end

    if offset == Moby.offset.rotation.x then
        self:ChangeAttributeForOtherPlayers(player, Moby.offset.rotation.x, 4, newValue, true)
    end
    if offset == Moby.offset.rotation.y then
        self:ChangeAttributeForOtherPlayers(player, Moby.offset.rotation.y, 4, newValue, true)
    end
    if offset == Moby.offset.rotation.z then
        self:ChangeAttributeForOtherPlayers(player, Moby.offset.rotation.z, 4, newValue, true)
    end
end 