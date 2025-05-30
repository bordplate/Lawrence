HovenInfobot = class("HovenInfobot", HybridMoby)

function HovenInfobot:initialize(level, uid)
    HybridMoby.initialize(self, level, uid)

    self.dead = false

    self:MonitorAttribute(Moby.offset.state, 1)
    self:MonitorPVar(0x2a * 4, 4)
    
    self.maxLocation = 0
    
    -- Positions to load if a player dies
    self.positions = {
        { x = 291.6, y = 320.1, z = 41 },
        { x = 255.0, y = 290.9, z = 41 },
        { x = 293.9, y = 288.0, z = 41 },
        { x = 293.4, y = 267.4, z = 45 },
        { x = 306.7, y = 244.0, z = 45 },
        { x = 307.1, y = 228.4, z = 68.5 },
    }
end

function HovenInfobot:OnAttributeChange(player, offset, oldValue, newValue)
    if offset == Moby.offset.state then
        if newValue == 4 then
            self:ChangeAttributeForOtherPlayers(player, Moby.offset.state, 1, 4)
        end
    end
    
    
end

function HovenInfobot:OnPVarChange(player, offset, oldValue, newValue)
    if offset == 0x2a * 4 then
        if newValue == 0 and self.maxLocation > 0 then
            player:ChangeMobyPVar(self.UID, 0x2a * 4, 4, self.maxLocation)
            
            player:ChangeMobyAttribute(self.UID, Moby.offset.position.x, 4, self.positions[self.maxLocation].x, true)
            player:ChangeMobyAttribute(self.UID, Moby.offset.position.y, 4, self.positions[self.maxLocation].y, true)
            player:ChangeMobyAttribute(self.UID, Moby.offset.position.z, 4, self.positions[self.maxLocation].z, true)
        end
        
        if newValue > self.maxLocation then
            print("New max location for Hoven infobot: " .. newValue)
            self.maxLocation = newValue
        end
    end
end 