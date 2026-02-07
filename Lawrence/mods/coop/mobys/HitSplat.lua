HitSplat = class('HitSplat', Entity)

function HitSplat:initialize(damage, level, x, y, z)
    Entity.initialize(self, nil)
    
    self.x = x + (math.random(-1, 1) / 2)
    self.y = y + (math.random(-1, 1) / 2)
    self.z = z + 0.5 + (math.random(-10, 25) / 100)
    
    self.ticks = 0
    self.alpha = 0xc0

    self.splatLabel = TextAreaElement(0, 0, 0, 0)
    self.splatLabel:SetWorldPosition(self.x, self.y, self.z)
    self.splatLabel.DrawsBackground = false
    self.splatLabel.TextSize = 0.6
    self.splatLabel.Text = "" .. damage
    self.splatLabel.Alignment = 1
    self.splatLabel.WorldSpaceFlags = 3
    self.splatLabel.TextColor = RGBA(255, 0, 0, self.alpha)
    self.splatLabel.HasShadow = false

    level:AddViewElement(self.splatLabel)
end

function HitSplat:OnTick()
    self.ticks = self.ticks + 1

    if self.ticks > 60 then
        self.splatLabel:Delete()
        self:Delete()
        
        return
    end

    if self.ticks > 30 then
        self.alpha = self.alpha - 10
        if self.alpha < 0 then
            self.alpha = 0
        end
        
        self.splatLabel.TextColor = RGBA(255, 0, 0, self.alpha)
    end
    
    self.z = self.z + 0.01
    
    self.splatLabel:SetWorldPosition(self.x, self.y, self.z)
end
