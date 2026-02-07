DoorRight = class('DoorRight', Moby)

function DoorRight:initialize(internalEntity)
    Moby.initialize(self, internalEntity)

    self:SetOClass(768)
    self.scale = 0.22
    self.players = 0
    
    self.shouldOpen = false
    self.openLevel = 0.0
end

function DoorRight:OnTick()
    if self.shouldOpen and self.openLevel < 30 then
        self.openLevel = self.openLevel + 1

        self:SetPosition(VectorAdd(self:Position(), VectorScale(self:Forward(), 0.1)))
    end
end
