PistonSeesaw = class('PistonSeesaw', Moby)

function PistonSeesaw:initialize(internalEntity)
    Moby.initialize(self, internalEntity)

    self:SetOClass(715)
    self.MovablePlatform = true
    self.modeBits = 0x20
    self.scale = 0.3
    self.players = 0
    
    self.height = 2.0
    
    self.linkedPiston = null
    self.controller = false
end

function PistonSeesaw:OnTick()
    if self.linkedPiston == null then
        return
    end
    
    if self.players > 0 then
        if self.height > 0.0 then
            self.height = self.height - 0.1

            self.z = self.z - 0.1
            
            self.linkedPiston.z = self.linkedPiston.z + 0.25
        end
    else
        if self.height < 2.0 then
            self.height = self.height + 0.1
            
            self.z = self.z + 0.1
            
            self.linkedPiston.z = self.linkedPiston.z - 0.25
        end
    end
end

function PistonSeesaw:OnStandingPlayer(player)
    self.players = self.players + 1
end

function PistonSeesaw:OnRemovedStandingPlayer(player)
    self.players = self.players - 1
end

