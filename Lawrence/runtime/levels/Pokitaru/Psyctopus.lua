Psyctopus = class("Psyctopus", HybridMoby)

function Psyctopus:initialize(level, uid)
    HybridMoby.initialize(self, level, uid)
    
    self.dead = false
    
    self:MonitorAttribute(Moby.offset.state, 1)
end

function Psyctopus:OnAttributeChange(player, offset, oldValue, newValue)
    if offset == Moby.offset.state then
        if newValue > 255 then
            return
        end
        
        if newValue == 8 then
            self.dead = true
            self:ChangeAttributeForOtherPlayers(player, Moby.offset.state, 1, newValue)
        end

        if self.dead and newValue ~= 8 and newValue ~= 253 then
            player:ChangeMobyAttribute(self.UID, Moby.offset.state, 1, 8)
        end
    end
end
