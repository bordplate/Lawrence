PistonActivator = class('PistonActivator', Moby)

function PistonActivator:initialize(internalEntity)
    Moby.initialize(self, internalEntity)

    self:SetOClass(280)
    self.modeBits = 0x4020
    
    self.pistons = {}

    self.scale = 0.1

    self.redTint = 0

    self.damageCooldown = 0
end

function PistonActivator:OnHit(moby, sourceOClass, damage)
    if self.damageCooldown > 0 then
        return
    end
    
    self.damageCooldown = 60

    for _, piston in ipairs(self.pistons) do
        piston.active = not piston.active
    end
    
    self.redTint = 255
end

function PistonActivator:OnTick()
    self.damageCooldown = self.damageCooldown - 1

    if self.damageCooldown > 0 then
        self.rotZ = self.rotZ + 5
    end

    if self.redTint > 128 then
        self.redTint = self.redTint - 2
        if self.redTint <= 128 then
            self:SetColor(128, 128, 128)
        else
            self:SetColor(self.redTint, 128, 128)
        end
    end
end 