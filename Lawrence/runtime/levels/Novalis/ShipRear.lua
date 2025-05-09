ShipRear = class("ShipRear", HybridMoby)

function ShipRear:initialize(level, uid, playerShip)
    print("Initializing ShipRear")
    HybridMoby.initialize(self, level, uid)
    
    self:MonitorAttribute(Moby.offset.state, 1)
end

function ShipRear:OnAttributeChange(player, offset, oldValue, newValue)
    if offset == Moby.offset.state then
        print("ShipRear state from " .. oldValue .. " changed to " .. newValue .. " by " .. player:Username())

        if newValue == 8 then
            self:ChangeAttributeForOtherPlayers(player, Moby.offset.state, 1, newValue)
        end
        if newValue == 5 then
            self:ChangeAttributeForOtherPlayers(player, Moby.offset.state, 1, newValue)
        end
    end
end
