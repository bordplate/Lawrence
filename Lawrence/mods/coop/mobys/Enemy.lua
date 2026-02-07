require 'mobys.HitSplat'

Enemy = class('Enemy', Moby)

function Enemy:initialize(internalEntity)
    Moby.initialize(self, internalEntity)

    self:SetOClass(1246)
    self.modeBits = 0x4020

    self.scale = 0.25
    
    self.redTint = 0
    
    self.damageCooldown = 0
    self.dead = false
    
    self.maxHealth = 10000000
    self.health = 1000000

    --self.healthLabel = TextAreaElement(0, 0, 0, 0)
    --self.healthLabel.DrawsBackground = false
    --self.healthLabel.Text = "" .. self.health .. "/" .. self.maxHealth
    --self.healthLabel.TextSize = 0.9
    --self.healthLabel.Alignment = 1
    --self.healthLabel.WorldSpaceFlags = 3
    --self.healthLabel.HasShadow = true

    --self:Level():AddViewElement(self.healthLabel)
end

function Enemy:OnHit(moby, sourceOClass, damage)
    if self.damageCooldown > 0 then
        return
    end
    
    --print("Damage: " .. moby.lastDamageDealt)
    
    local dealt = damage
    self.health = self.health - dealt

    if self.health <= 0 then
        self.health = 0
        self.dead = true

        self.damageCooldown = 0
        self.AnimationId = 12
    else
        self.damageCooldown = 0
        self.AnimationId = 8
    end
    
    HitSplat(dealt, self:Level(), self.x, self.y, self.z)
    
    --self.healthLabel.Text = "" .. self.health .. "/" .. self.maxHealth
    
    self.redTint = 255
end 

function Enemy:OnTick()
    self.damageCooldown = self.damageCooldown - 1

    --self.healthLabel:SetWorldPosition(self.x, self.y, self.z + 2)

    if self.AnimationId == 8 and self.damageCooldown <= 0 then
        self.AnimationId = 0
    end
    if self.AnimationId == 12 and self.damageCooldown <= 0 then
        --self.healthLabel:Delete()
        self:Delete()
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