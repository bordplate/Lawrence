Button = class("Button", HybridMoby)

function Button:initialize(level, uid)
    HybridMoby.initialize(self, level, uid)

    self:MonitorAttribute(Moby.offset.state, 1)
end

function Button:OnAttributeChange(player, offset, oldValue, newValue)
    if offset == Moby.offset.state then
        print("Button state from " .. oldValue .. " changed to " .. newValue .. " by " .. player:Username())

        if newValue == 2 then
            self:ChangeAttributeForOtherPlayers(player, Moby.offset.state, 1, 2)
            self:ChangeAttributeForOtherPlayers(player, 0xbc, 1, 1)
            self:ChangeAttributeForOtherPlayers(player, 0x90,  4, 0x80208020)
        end
    end
end

function Button:OnPVarChange(player, offset, oldValue, newValue)

end
