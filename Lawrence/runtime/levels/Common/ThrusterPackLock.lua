ThrusterPackLock = class("ThrusterPackLock", HybridMoby)

function ThrusterPackLock:initialize(level, uid)
    print("Initializing ThrusterPackLock")
    HybridMoby.initialize(self, level, uid)

    self:MonitorAttribute(Moby.offset.state, 1)
    self:MonitorAttribute(Moby.offset.position.z, 4, true)
end

function ThrusterPackLock:OnAttributeChange(player, offset, oldValue, newValue)
    if offset == Moby.offset.state then
        print("ThrusterPackLock state from " .. oldValue .. " changed to " .. newValue .. " by " .. player:Username())
        if newValue == 2 then
            self:ChangeAttributeForOtherPlayers(player, Moby.offset.state, 1, 2)
        end
    end
    
    if offset == Moby.offset.position.z then
        self:ChangeAttributeForOtherPlayers(player, Moby.offset.position.z, 4, newValue, true)
    end
end
