Puffoid = class("Puffoid", HybridMoby)

function Puffoid:initialize(level, uid)
    print("Initializing Puffoid")
    HybridMoby.initialize(self, level, uid)
    
    self.dead = false

    self:MonitorAttribute(Moby.offset.state, 1)
end

function Puffoid:OnAttributeChange(player, offset, oldValue, newValue)
    if offset == Moby.offset.state then
        if newValue > 255 then
            return
        end

        if newValue == 11 then
            self.dead = true
            self:ChangeAttributeForOtherPlayers(player, Moby.offset.state, 1, newValue)
        end

        if newValue == 253 then
            self.dead = true
        end
        
        if self.dead and newValue ~= 11 and newValue ~= 253 then
            player:ChangeMobyAttribute(self.UID, Moby.offset.state, 1, 11)
        end
    end
end
