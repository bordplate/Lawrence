DoorLeft = class('DoorLeft', Moby)

function DoorLeft:initialize(internalEntity)
    Moby.initialize(self, internalEntity)

    self:SetOClass(769)
    self.scale = 0.22
    self.players = 0
    
    self.shouldOpen = false
    self.openLevel = 0.0
end

function DoorLeft:OnTick()
    if self.shouldOpen and self.openLevel < 30 then
        self.openLevel = self.openLevel + 1

        self:SetPosition(VectorSub(self:Position(), VectorScale(self:Forward(), 0.1)))
    end
end
