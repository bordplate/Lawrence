PipeButton = class('PipeButton', Moby)

function PipeButton:initialize(internalEntity)
    Moby.initialize(self, internalEntity)

    self:SetOClass(778)
    self.modeBits = 0x5020
    self.scale = 0.07
    
    self.hit = false
    
    self.doorLeft = nil
    self.doorRight = nil
end

function PipeButton:OnTick()

end

function PipeButton:OnHit(attacker)
    print("Attack!")
    if not self.hit then
        self.doorLeft.shouldOpen = true
        self.doorRight.shouldOpen = true
        self:SetOClass(779)
    end
end
