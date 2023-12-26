BarrierMoby = class("BarrierMoby", Moby)

function BarrierMoby:initialize(internalEntity)
    Moby.initialize(self, internalEntity)
    
    self:SetOClass(1964) -- Veldin barrier
    
    self.scale = 1
    self.rotZ = 40
end
