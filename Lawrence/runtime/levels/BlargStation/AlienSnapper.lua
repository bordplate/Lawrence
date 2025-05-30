AlienSnapper = class("AlienSnapper", HybridMoby)

function AlienSnapper:initialize(level, uid)
    HybridMoby.initialize(self, level, uid)

    self:MonitorAttribute(Moby.offset.state, 1)
    
    self.dead = false
end

function AlienSnapper:OnAttributeChange(player, offset, oldValue, newValue)
    if offset == Moby.offset.state then
        if self.dead and newValue < 255 and newValue ~= 17 and newValue ~= 253 then
            player:ChangeMobyAttribute(self.UID, Moby.offset.state, 1, 17)
        end 
        
        print("AlienSnapper state from " .. oldValue .. " changed to " .. newValue .. " by " .. player:Username())

        if newValue == 17 then
            self.dead = true
            self:ChangeAttributeForOtherPlayers(player, Moby.offset.state, 1, newValue)
        end
    end
end