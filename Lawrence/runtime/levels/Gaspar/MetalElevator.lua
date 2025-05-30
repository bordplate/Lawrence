MetalElevator = class("MetalElevator", HybridMoby)

function MetalElevator:initialize(level, uid)
    HybridMoby.initialize(self, level, uid)

    --self:MonitorAttribute(Moby.offset.state, 1)
    self:MonitorAttribute(Moby.offset.position.x, 4, true)
    self:MonitorAttribute(Moby.offset.position.y, 4, true)
    self:MonitorAttribute(Moby.offset.position.z, 4, true)
    self:MonitorAttribute(Moby.offset.rotation.x, 4, true)
    self:MonitorAttribute(Moby.offset.rotation.y, 4, true)
    self:MonitorAttribute(Moby.offset.rotation.z, 4, true)
end

function MetalElevator:OnAttributeChange(player, offset, oldValue, newValue)
    if offset == Moby.offset.state then
        print("MetalElevator state from " .. oldValue .. " changed to " .. newValue .. " by " .. player:Username())

        if newValue ~= 0 then
            self:ChangeAttributeForOtherPlayers(player, Moby.offset.state, 1, newValue)
        end
    end

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

function MetalElevator:OnPVarChange(player, offset, oldValue, newValue)

end
